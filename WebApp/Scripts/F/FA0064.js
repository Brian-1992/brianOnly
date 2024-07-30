Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.require(['WEBAPP.utils.Common']);

Ext.onReady(function () {
    // 拆解網址參數
    //Ext.getUrlParam = function (param) {
    //    var params = Ext.urlDecode(location.search.substring(1));
    //    return param ? params[param] : params;
    //};
    var T1GetExcel = '/api/FA0064/Excel';
    var windowHeight = $(window).height();
    var windowWidth = $(window).width();
    var pYear = 109;
    var ppYear = 109;
    //var view_all = Ext.getUrlParam('view_all');

    Ext.override(Ext.grid.column.Column, { menuDisabled: true });
    var mLabelWidth = 80;
    var mWidth = 200;

    function setYrDefault() {
        Ext.Ajax.request({
            url: '/api/FA0064/GetYrDefault',
            method: reqVal_g,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    var temp = data.etts[0]
                    pYear = temp.TEXT;
                    ppYear = temp.VALUE;
                    if (temp.HOSP_CODE != "0") {
                        Ext.getCmp('TR_INV_QTY').setHidden(true); //載入資料時檢查醫院是否國軍(控制轉讓北門是否顯示)
                        Ext.getCmp('CNV_TR_INV_QTY').setHidden(true);
                    }
                }
            },
            failure: function (response, options) {

            }
        });
    }
    setYrDefault();

    Ext.define('T1Model', {
        extend: 'Ext.data.Model',
        fields: [
            'MMCODE',              //院內碼
            'MMNAME_E',            //英文品名
            'MMNAME_C',            //中文品名
            'E_SCIENTIFICNAME',    //成份名稱           
            'MED_LICENSE',         //許可證字號
            'E_RESTRICTCODE',      //管制級別
            'CANCEL_ID',           //是否全院停用
            'DELI_QTY',            //進貨量
            'TR_INV_QTY',          //轉讓北門
            'APPQTY',              //退貨量
            'PINVQTY',             //結存量
            'BASE_UNIT',           //計量單位
            'CNV_RATE',            //換算率
            'CNV_DELI_QTY',        //換算後進貨量
            'CNV_TR_INV_QTY',      //換算後轉讓北門
            'CNV_APPQTY',          //換算後退貨量
            'CNV_PINVQTY',         //換算後結存量
            'DECLARE_UI1'         //申報計量單位
        ]
    });

    var T1Store = Ext.create('Ext.data.Store', {
        model: 'T1Model',
        pageSize: 20, // 每頁顯示筆數
        remoteSort: true,
        //sorters: [{ property: 'CREATE_TIME', direction: 'ASC' }], // 預設排序欄位
        proxy: {
            type: 'ajax',
            timeout: 300000,
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/FA0064/GetData',
            reader: {
                type: 'json',
                rootProperty: 'etts',
                totalProperty: 'rc'
            }
        },
        listeners: {
            beforeload: function (store, options) {
                var np = {
                    p0: T1Query.getForm().findField('P0').rawValue,
                    p1: T1Query.getForm().findField('P1').rawValue, 
                };
                Ext.apply(store.proxy.extraParams, np);
            }
        }
    });

    var T1Query = Ext.widget({
        xtype: 'form',
        layout: 'form',
        border: false,
        autoScroll: true, // 若為true,容器內容超出容器大小時出現捲動條;若為false超出容器的部分會被裁切掉
        fieldDefaults: {
            xtype: 'textfield',
            labelAlign: 'right',
            labelWidth: mLabelWidth,
            width: mWidth
        },
        items: [{
            xtype: 'panel',
            id: 'PanelP1',
            border: false,
            layout: 'hbox',
            items: [
                {
                    xtype: 'datefield',
                    fieldLabel: '日期(年月日)',
                    name: 'P0',
                    id: 'P0',
                    width: 160,
                    padding: '0 4 0 4',
                    //format: 'Xm',
                    fieldCls: 'required',
                    value: new Date(new Date() - 1 * 24 * 60 * 60 * 1000),
                    maxValue: new Date(new Date() - 1 * 24 * 60 * 60 * 1000)
                },
                {
                    xtype: 'datefield',
                    fieldLabel: '至',
                    name: 'P1',
                    id: 'P1',
                    // enforceMaxLength: true,
                    //maxLength: 5,
                    //minLength: 5,
                    //regexText: '請填入民國年月',
                    //regex: /\d{5,5}/,
                    fieldCls: 'required',
                    labelWidth: 8,
                    labelSeperator: '',
                    width: 88,
                    padding: '0 4 0 4',
                    //format: 'Xm',
                    //value: new Date()
                    value: new Date(new Date() - 1 * 24 * 60 * 60 * 1000),
                    maxValue: new Date(new Date() - 1 * 24 * 60 * 60 * 1000)
                }, {
                    xtype: 'button',
                    text: '查詢',
                    margin: '0 5 0 20',
                    handler: function () {
                        if (!T1Query.getForm().findField('P0').getValue() ||
                            !T1Query.getForm().findField('P1').getValue()) {
                            Ext.Msg.alert('提醒', '<span style="color:red">日期</span>為必填');
                            return;
                        }
                        if (T1Query.getForm().isValid() == false) {
                            Ext.Msg.alert('提醒', '日期不可大於昨天');
                            return;
                        }

                        var start_year = T1Query.getForm().findField('P0').rawValue.substring(0,3);
                        var end_year = T1Query.getForm().findField('P1').rawValue.substring(0, 3);

                        if (start_year != end_year) {
                            Ext.Msg.alert('提醒', '起迄日期年份需相同');
                            return;
                        }

                        T1Load();
                    }
                }, {
                    xtype: 'button',
                    text: '清除',
                    handler: function () {
                        var f = this.up('form').getForm();

                        f.reset();

                        msglabel('訊息區:');
                    }
                },
                {
                    xtype: 'button',
                    text: '管制藥申報換算率',
                    margin: '0 0 0 20',
                    handler: function () {
                        T2Query.getForm().findField('P0').setValue(pYear);
                        editWindow.show();
                        T2Load();
                    }
                }
            ]
        },
        ]
    });

    // toolbar,包含換頁、新增/修改/刪除鈕
    var T1Tool = Ext.create('Ext.PagingToolbar', {
        store: T1Store,
        displayInfo: true,
        border: false,
        plain: true,
        buttons: [
            {
                text: '匯出',
                handler: function () {
                    if (!T1Query.getForm().findField('P0').getValue() ||
                        !T1Query.getForm().findField('P1').getValue()) {
                        Ext.Msg.alert('提醒', '<span style="color:red">日期</span>不可空白');
                        return;
                    }

                    var p = new Array();
                    p.push({ name: 'FN', value: '1-4級管制藥年度結存量報表.xls' });
                    p.push({ name: 'start_date', value: T1Query.getForm().findField('P0').rawValue });
                    p.push({ name: 'end_date', value: T1Query.getForm().findField('P1').rawValue });
                    PostForm(T1GetExcel, p);
                    msglabel('匯出完成');
                }
            },
        ]
    });

    // 查詢結果資料列表
    var T1Grid = Ext.create('Ext.grid.Panel', {
        store: T1Store,
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T1',
        dockedItems: [{
            dock: 'top',
            xtype: 'toolbar',
            layout: 'fit',
            items: [T1Query]
        }, {
            dock: 'top',
            xtype: 'toolbar',
            items: [T1Tool]
        }],
        columns: [{
            xtype: 'rownumberer'
        }, {
            text: "院內碼",
            dataIndex: 'MMCODE',
            width: 80
        }, {
            text: "英文品名",
            dataIndex: 'MMNAME_E',
            width: 130
        }, {
            text: "中文品名",
            dataIndex: 'MMNAME_C',
            width: 130
        }, {
            text: "成份名稱",
            dataIndex: 'E_SCIENTIFICNAME',
            width: 130
        }, {
            text: "許可證字號",
            dataIndex: 'MED_LICENSE',
            width: 80
        }, {
            text: "管制級別",
            dataIndex: 'E_RESTRICTCODE',
            width: 70,
        }, {
            text: "是否全院停用",
            dataIndex: 'CANCEL_ID',
            width: 100,
            style: 'text-align:left',
            align: 'right',
        }, {
            text: "進貨量",
            dataIndex: 'DELI_QTY',
            width: 70,
        }, {
            id: 'TR_INV_QTY',
            itemId: 'TR_INV_QTY',
            text: "轉讓北門",
            dataIndex: 'TR_INV_QTY',
            width: 90
        }, {
            text: "退貨量",
            dataIndex: 'APPQTY',
            width: 70,
        }, {
            text: "結存量",
            dataIndex: 'PINVQTY',
            width: 80
        }, {
            text: "計量單位",
            dataIndex: 'BASE_UNIT',
            width: 80,
        }, {
            text: "換算率",
            dataIndex: 'CNV_RATE',
            width: 80
        }, {
            text: "換算後進貨量",
            dataIndex: 'CNV_DELI_QTY',
            width: 100
        }, {
            id: 'CNV_TR_INV_QTY',
            itemId: 'CNV_TR_INV_QTY',
            text: "換算後轉讓北門",
            dataIndex: 'CNV_TR_INV_QTY',
            width: 100
        }, {
            text: "換算後退貨量",
            dataIndex: 'CNV_APPQTY',
            width: 110
        }, {
            text: "換算後結存量",
            dataIndex: 'CNV_PINVQTY',
            width: 110
        }, {
            text: "申報計量單位",
            dataIndex: 'DECLARE_UI1',
            width: 110
        }]
    });

    function T1Load() {
        T1Tool.moveFirst();
        msglabel('訊息區:');
    }

    //#region editWindow
    Ext.define('T2Model', {
        extend: 'Ext.data.Model',
        fields: [
            'DCLYR', 
            'MMCODE',           
            'MED_LICENSE',           
            'DECLARE_UI',          
            'CNV_RATE',            
            'CREATE_TIME',        
            'CREATE_USER'
        ]
    });

    var T2Store = Ext.create('Ext.data.Store', {
        model: 'T2Model',
        pageSize: 20, // 每頁顯示筆數
        remoteSort: true,
        //sorters: [{ property: 'CREATE_TIME', direction: 'ASC' }], // 預設排序欄位
        proxy: {
            type: 'ajax',
            timeout: 180000,
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/FA0064/GetDcluicnvs',
            reader: {
                type: 'json',
                rootProperty: 'etts',
                totalProperty: 'rc'
            }
        },
        listeners: {
            beforeload: function (store, options) {
                var np = {
                    dclyr: T2Query.getForm().findField('P0').getValue(),
                    mmcode: T2Query.getForm().findField('P1').getValue(),
                    med_license: T2Query.getForm().findField('P2').getValue(),
                };
                Ext.apply(store.proxy.extraParams, np);
            }
        }
    });
    var T2QueryMMCode = Ext.create('WEBAPP.form.MMCodeCombo', {
        name: 'P1',
        fieldLabel: '院內碼',
        labelAlign: 'right',
        limit: 10, //限制一次最多顯示10筆
        queryUrl: '/api/FA0064/GetMmcodeCombo', //指定查詢的Controller路徑
        extraFields: ['MMNAME_C', 'MMNAME_E', 'BASE_UNIT'], //查詢完會回傳的欄位
        getDefaultParams: function () { //查詢時Controller固定會收到的參數
            //return {
            //    p1: T1Form.getForm().findField('DOCNO').getValue()
            //};
        },
        listeners: {
            select: function (c, r, i, e) {
                //選取下拉項目時，顯示回傳值
            }
        }
    });
    var T2Query = Ext.widget({
        xtype: 'form',
        layout: 'form',
        border: false,
        autoScroll: true, // 若為true,容器內容超出容器大小時出現捲動條;若為false超出容器的部分會被裁切掉
        fieldDefaults: {
            xtype: 'textfield',
            labelAlign: 'right',
            labelWidth: mLabelWidth,
            width: mWidth
        },
        items: [{
            xtype: 'panel',
            border: false,
            layout: 'hbox',
            items: [
                {
                    xtype: 'numberfield',
                    fieldLabel: '申報年度',
                    name: 'P0',
                    width: 160,
                    padding: '0 4 0 4',
                    //format: 'Xm',
                    minValue:109,
                    fieldCls: 'required',
                    hideTrigger: true,
                    decimalPrecision: 0,
                    value: pYear
                },
                T2QueryMMCode, 
                {
                    fieldLabel: '許可證字號',
                    name: 'P2',
                    padding: '0 4 0 4',
                    xtype: 'textfield',
                    labelAlign: 'right',
                    labelWidth: mLabelWidth,
                    width: mWidth
                }, {
                    xtype: 'button',
                    text: '查詢',
                    margin: '0 5 0 20',
                    handler: function () {
                        if (!T1Query.getForm().findField('P0').getValue()) {
                            Ext.Msg.alert('提醒', '<span style="color:red">申報年度</span>為必填');
                            return;
                        }

                        T2Load();
                    }
                }, {
                    xtype: 'button',
                    text: '清除',
                    handler: function () {
                        var f = this.up('form').getForm();

                        f.reset();
                        f.findField('P0').setValue(pYear);

                        msglabel('訊息區:');
                    }
                },
                {
                    xtype: 'filefield',
                    buttonText: '匯入',
                    buttonOnly: true,
                    padding: '0 4 0 0',
                    id:'upload',
                    width: 72,
                    listeners: {
                        change: function (widget, value, eOpts) {
                            var files = event.target.files;
                            if (!files || files.length == 0) return; // make sure we got something
                            var file = files[0];
                            var ext = this.value.split('.').pop();
                            if (!/^(xls|xlsx|XLS|XLSX)$/.test(ext)) {
                                Ext.MessageBox.alert('提示', '僅支持讀取xlsx和xls格式！');
                                Ext.getCmp('import').fileInputEl.dom.value = '';
                                msglabel('');
                            } else {
                                var myMaskEditWindow = new Ext.LoadMask(Ext.getCmp('editWindow'), { msg: '處理中...' });
                                msglabel("已選擇檔案");
                                myMaskEditWindow.show();
                                var formData = new FormData();
                                formData.append("file", file);
                                var ajaxRequest = $.ajax({
                                    type: "POST",
                                    url: "/api/FA0064/Upload",
                                    data: formData,
                                    processData: false,
                                    //必須false才會自動加上正確的Content-Type
                                    contentType: false,
                                    success: function (data, textStatus, jqXHR) {
                                        myMaskEditWindow.hide();
                                        if (!data.success) {
                                            
                                            Ext.MessageBox.alert("提示", data.msg);
                                            msglabel("訊息區:");
                                            Ext.getCmp('upload').setRawValue("");
                                            Ext.getCmp('upload').fileInputEl.dom.value = '';
                                        }
                                        else {
                                            msglabel("訊息區:資料匯入成功");
                                            Ext.getCmp('upload').setRawValue("");
                                            Ext.getCmp('upload').fileInputEl.dom.value = '';


                                            T2Query.getForm().findField('P0').setValue(pYear);
                                            T2Load();
                                        }
                                    },
                                    error: function (jqXHR, textStatus, errorThrown) {
                                        myMaskEditWindow.hide();
                                        Ext.Msg.alert('失敗', 'Ajax communication failed');
                                        //T1Query.getForm().findField('send').fileInputEl.dom.value = '';
                                        Ext.getCmp('upload').setRawValue("");
                                    }
                                });
                            }
                        }

                    }

                },
            ]
        },
        ]
    });

    // toolbar,包含換頁、新增/修改/刪除鈕
    var T2Tool = Ext.create('Ext.PagingToolbar', {
        store: T2Store,
        displayInfo: true,
        border: false,
        plain: true,
        buttons: [
            {
                text: '複製轉新增',
                handler: function () {
                    T3Form.getForm().findField('P0').setValue(ppYear);
                    T3Form.getForm().findField('P1').setValue(pYear);
                    copyWindow.show();

                }
            },
            {
                text: '匯出',
                handler: function () {
                    if (!T2Query.getForm().findField('P0').getValue()) {
                        Ext.Msg.alert('提醒', '<span style="color:red">申報年度</span>不可空白');
                        return;
                    }

                    var p = new Array();
                    p.push({ name: 'FN', value: '管制藥申報換算率.xls' });
                    p.push({ name: 'dclyr', value: T2Query.getForm().findField('P0').rawValue });
                    p.push({ name: 'mmcode', value: T2Query.getForm().findField('P1').rawValue });
                    p.push({ name: 'med_license', value: T2Query.getForm().findField('P2').rawValue });
                    PostForm('/api/FA0064/ExcelDcluicnv', p);
                    msglabel('匯出完成');
                }
            },
        ]
    });

    var T2Grid = Ext.create('Ext.grid.Panel', {
        store: T2Store,
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T1',
        dockedItems: [{
            dock: 'top',
            xtype: 'toolbar',
            layout: 'fit',
            items: [T2Query]
        }, {
            dock: 'top',
            xtype: 'toolbar',
            items: [T2Tool]
        }],
        columns: [{
            xtype: 'rownumberer'
        }, {
            text: "申報年度",
            dataIndex: 'DCLYR',
            width: 70
        }, {
            text: "院內碼",
            dataIndex: 'MMCODE',
            width: 80
        }, {
                text: "許可證字號",
                dataIndex: 'MED_LICENSE',
            width: 110
        }, {
            text: "申報計量單位",
            dataIndex: 'DECLARE_UI',
            width: 110
        }, {
                text: "換算率",
                dataIndex: 'CNV_RATE',
                style: 'text-align:left',
                align: 'right',
            width: 80
        }, {
                text: "建立時間",
                dataIndex: 'CREATE_TIME',
            width: 110,
        }, {
                text: "建立人員",
                dataIndex: 'CREATE_USER',
            width: 90,
        }]
    });

    function T2Load(clearMsg) {
        T2Tool.moveFirst();
        if (clearMsg) {
            msglabel('');
        }
    }

    var editWindow = Ext.create('Ext.window.Window', {
        renderTo: Ext.getBody(),
        modal: true,
        items: [T2Grid],
        width: windowWidth,
        height: windowHeight,
        id:'editWindow',
        xtype: 'form',
        layout: 'form',
        resizable: true,
        draggable: true,
        closable: false,
        layout: {
            type: 'fit',
            padding: 0
        },
        title: "管制藥申報換算率",
        buttons: [
            //{
            //    text: '完成',
            //    handler: function () {
            //        checkSetRatioEmpty();
            //        editWindow.hide();
            //    }
            //},
            {
                text: '關閉',
                handler: function () {
                    editWindow.hide();
                }
            }
        ]
    });
    editWindow.hide();
    //#endregion

    //#region copyWindow

    function copy() {
        if (!T3Form.getForm().findField('P0').getValue() ||
            !T3Form.getForm().findField('P1').getValue()) {
            Ext.Msg.alert('錯誤', '年分為必填');
            return;
        }

        var myMaskT3Form = new Ext.LoadMask(Ext.getCmp('T3Form'), { msg: '處理中...' });
        myMaskT3Form.show();
         Ext.Ajax.request({
            url: '/api/FA0064/Copy',
            method: reqVal_p,
            params: {
                from: T3Form.getForm().findField('P0').getValue(),
                to: T3Form.getForm().findField('P1').getValue(),
            },
            success: function (response) {
                var data = Ext.decode(response.responseText);
                myMaskT3Form.hide();

                if (data.success == false)
                {
                    Ext.Msg.alert('錯誤', data.msg);
                    return;
                }

                copyWindow.hide();

                T2Query.getForm().findField('P0').setValue(T3Form.getForm().findField('P1').getValue());
                T2Query.getForm().findField('P1').setValue('');
                T2Query.getForm().findField('P2').setValue('');
                msglabel('複製轉新增完成');
                T2Load();
            },
            failure: function (response, options) {

            }
        });
    }

    var T3Form = Ext.widget({
        xtype: 'form',
        layout: 'form',
        id:'T3Form',
        border: false,
        autoScroll: true, // 若為true,容器內容超出容器大小時出現捲動條;若為false超出容器的部分會被裁切掉
        fieldDefaults: {
            xtype: 'textfield',
            labelAlign: 'right',
            labelWidth: mLabelWidth,
            width: mWidth
        },
        items: [{
            xtype: 'panel',
            border: false,
            layout: 'hbox',
            items: [
                {
                    xtype: 'numberfield',
                    fieldLabel: '自',
                    name: 'P0',
                    width: 160,
                    padding: '0 4 0 4',
                    //format: 'Xm',
                    minValue: 109,
                    fieldCls: 'required',
                    hideTrigger: true,
                    decimalPrecision: 0,
                    value: pYear
                }
            ]
        },
            {
                xtype: 'panel',
                border: false,
                layout: 'hbox',
                items: [
                    {
                        xtype: 'numberfield',
                        fieldLabel: '至',
                        name: 'P1',
                        width: 160,
                        padding: '0 4 0 4',
                        //format: 'Xm',
                        minValue: 109,
                        fieldCls: 'required',
                        hideTrigger: true,
                        decimalPrecision: 0,
                        value: pYear
                    },

                ]
            },
        ]
    });

    var copyWindow = Ext.create('Ext.window.Window', {
        renderTo: Ext.getBody(),
        modal: true,
        items: [T3Form],
        xtype: 'form',
        layout: 'form',
        resizable: false,
        draggable: false,
        closable: false,
        layout: {
            type: 'fit',
            padding: 0
        },
        title: "複製轉新增",
        buttons: [
            {
                text: '確定',
                handler: function () {
                    copy();
                }
            },
            {
                text: '取消',
                handler: function () {
                    copyWindow.hide();
                }
            }
        ]
    });
    copyWindow.hide();

    //#endregion

    //#region viewport
    var viewport = Ext.create('Ext.Viewport', {
        renderTo: body,
        layout: {
            type: 'border',
            padding: 0
        },
        defaults: {
            split: true
        },
        items: [{
            itemId: 't1Grid',
            region: 'center',
            layout: 'fit',
            collapsible: false,
            title: '',
            border: false,
            items: [T1Grid]
        }
        ]
    });
    //#endregion
});