Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});
Ext.require(['WEBAPP.utils.Common']);
Ext.onReady(function () {
    //var reportUrl = '/Report/F/FA0055.aspx';
    var T1GetExcel = '/api/FA0063/Excel';
    // var T1Get = '/api/FA0055/All'; // 查詢(改為於store定義)
    var YMComboGet = '../../../api/FA0063/GetYMCombo';
    var windowHeight = $(window).height();
    var windowWidth = $(window).width();

    Ext.override(Ext.grid.column.Column, { menuDisabled: true });

    var YMQueryStore = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT']
    });
    function setYMComboData() {
        Ext.Ajax.request({
            url: YMComboGet,
            method: reqVal_g,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    var set_ym = data.etts;
                    if (set_ym.length > 0) {
                        for (var i = 0; i < set_ym.length; i++) {
                            YMQueryStore.add({ VALUE: set_ym[i].VALUE, TEXT: set_ym[i].TEXT });
                            if (i == 0) {
                                tmpYM = set_ym[i];
                            }
                        }
                    }
                    T1Query.getForm().findField('P0').setValue(tmpYM);  //預設第一筆最近年月
                    T1Query.getForm().findField('P1').setValue(tmpYM);  //預設第一筆最近年月
                }
            },
            failure: function (response, options) {

            }
        });
    }
    setYMComboData();

    function checkSetRatioEmpty() {
        Ext.Ajax.request({
            url: '/api/FA0063/CheckSetRatio',
            method: reqVal_g,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                console.log(response.responseText);
                Ext.getCmp('T1SearchBtn').enable();
                T1Query.getForm().findField('setRatioMsg').hide();
                Ext.getCmp('btnExcel').enable();
                Ext.getCmp('btnTxt').enable();
                if (data.msg == 'Y') {
                    Ext.getCmp('T1SearchBtn').disable();
                    T1Query.getForm().findField('setRatioMsg').show();
                    Ext.getCmp('btnExcel').disable();
                    Ext.getCmp('btnTxt').disable();
                }
            },
            failure: function (response, options) {

            }
        });
    }
    checkSetRatioEmpty();

    function getLastUploadtTime() {
        Ext.Ajax.request({
            url: '/api/FA0063/GetLastUploadTime',
            method: reqVal_g,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                T1Query.getForm().findField('lastUploadTime').setValue(data.msg);
            },
            failure: function (response, options) {

            }
        });
    }
    getLastUploadtTime();

    var mLabelWidth = 80;
    var mWidth = 180;

    // 查詢欄位
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
            xtype: 'container',
            layout: 'vbox',
            items: [
                {
                    xtype: 'panel',
                    id: 'PanelP1',
                    border: false,
                    width: '100%',
                    layout: 'hbox',
                    items: [
                        {
                            xtype: 'combo',
                            fieldLabel: '資料年月',
                            name: 'P0',
                            id: 'P0',
                            enforceMaxLength: true,
                            labelWidth: 60,
                            width: 140,
                            store: YMQueryStore,
                            displayField: 'TEXT',
                            valueField: 'VALUE',
                            queryMode: 'local',
                            anyMatch: true,
                            allowBlank: false,
                            fieldCls: 'required',
                            tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{TEXT}&nbsp;</div></tpl>',
                            editable: false
                        },
                        {
                            xtype: 'combo',
                            fieldLabel: '至',
                            name: 'P1',
                            id: 'P1',
                            labelSeparator: '',
                            enforceMaxLength: true,
                            labelWidth: 30,
                            width: 110,
                            store: YMQueryStore,
                            displayField: 'TEXT',
                            valueField: 'VALUE',
                            queryMode: 'local',
                            anyMatch: true,
                            allowBlank: false,
                            fieldCls: 'required',
                            tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{TEXT}&nbsp;</div></tpl>',
                            editable: false,
                        },
                        {
                            xtype: 'button',
                            text: '查詢',
                            id: 'T1SearchBtn',
                            margin: '0 0 0 20',
                            handler: function () {
                                if (!T1Query.getForm().findField('P0').getValue() || !T1Query.getForm().findField('P1').getValue()) {
                                    Ext.Msg.alert('提醒', '<span>年月</span>不可空白');
                                    return;
                                }

                                T1Load();
                                msglabel('');
                            }
                        },
                        {
                            xtype: 'button',
                            text: '清除',
                            margin: '0 5 0 5',
                            handler: function () {
                                var f = this.up('form').getForm();
                                f.reset();
                                getHospId();
                                f.findField('P0').focus(); // 進入畫面時輸入游標預設在P0
                            }
                        },
                        {
                            xtype: 'filefield',
                            name: 'uploadExcel',
                            id: 'uploadExcel',
                            buttonText: '匯入藥品及衛材價量調查品項',
                            buttonOnly: true,
                            padding: '0 4 0 0',
                           
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
                                        
                                        msglabel("已選擇檔案");
                                        myMaskViewport.show();
                                        var formData = new FormData();
                                        formData.append("file", file);
                                        var ajaxRequest = $.ajax({
                                            type: "POST",
                                            url: "/api/FA0063/Upload",
                                            data: formData,
                                            processData: false,
                                            //必須false才會自動加上正確的Content-Type
                                            contentType: false,
                                            success: function (data, textStatus, jqXHR) {
                                                myMaskViewport.hide();
                                                if (!data.success) {
                                                    Ext.MessageBox.alert("提示", data.msg);
                                                    msglabel("訊息區:");
                                                    Ext.getCmp('uploadExcel').fileInputEl.dom.value = '';
                                                }
                                                else {
                                                    msglabel("訊息區:資料匯入成功");
                                                    Ext.getCmp('uploadExcel').fileInputEl.dom.value = '';

                                                    // 設定倍率
                                                    Ext.Ajax.request({
                                                        url: '/api/FA0063/GetNhipsurvlistInit',
                                                        method: reqVal_p,
                                                        success: function (response) {
                                                            var data = Ext.decode(response.responseText);
                                                            msglabel('倍率設定完成');
                                                            myMaskViewport.hide();
                                                            if (data.success) {
                                                                T2Load();
                                                                editWindow.show();
                                                            } else {

                                                            }
                                                        },
                                                        failure: function (response, options) {
                                                            myMaskViewport.hide();
                                                        }
                                                    });
                                                }
                                            },
                                            error: function (jqXHR, textStatus, errorThrown) {
                                                myMaskViewport.hide();
                                                Ext.Msg.alert('失敗', 'Ajax communication failed');
                                                //T1Query.getForm().findField('send').fileInputEl.dom.value = '';
                                                Ext.getCmp('uploadExcel').setRawValue("");
                                            }
                                        });
                                    }
                                }

                            },
                            

                        }, {
                            xtype: 'displayfield',
                            fieldLabel: '上次上傳時間',
                            margin: '0 50 0 0',
                            labelWidth: 180,
                            width: 280,
                            value: '',
                            name: 'lastUploadTime'
                        }
                        

                    ]
                },
                {
                    xtype: 'panel',
                    id: 'PanelP2',
                    border: false,
                    width: '100%',
                    layout: 'hbox',
                    items: [
                        {
                            xtype: 'textfield',
                            fieldLabel: '醫事服務機構代號',
                            name: 'P2',
                            id: 'P2',
                            allowBlank: false,
                            fieldCls: 'required',
                            width: 230,
                            labelWidth:105
                        },
                        
                        {
                            xtype: 'button',
                            text: '更新醫事服務機構代號',
                            margin: '0 0 0 20',
                            handler: function () {
                                if (!T1Query.getForm().findField('P2').getValue()) {
                                    Ext.Msg.alert('提醒', '<span>醫事服務機構代號</span>不可空白');
                                    return;
                                }

                                updateHospId(T1Query.getForm().findField('P2').getValue());
                            }
                        }
                        
                        
                        ,
                        {
                            xtype: 'button',
                            text: '設定倍率',
                            margin: '0 5 0 5',
                            handler: function () {
                                myMaskViewport.show();
                                msglabel('倍率設定中');
                                //T2Load();
                                Ext.Ajax.request({
                                    url: '/api/FA0063/GetNhipsurvlistInit',
                                    method: reqVal_p,
                                    success: function (response) {
                                        var data = Ext.decode(response.responseText);
                                        msglabel('倍率設定完成');
                                        myMaskViewport.hide();
                                        if (data.success) {
                                            T2Load();
                                            editWindow.show();
                                        } else {

                                        }
                                    },
                                    failure: function (response, options) {
                                        myMaskViewport.hide();
                                    }
                                });
                            }
                        },
                        {
                            xtype: 'displayfield',
                            fieldLabel: '有未設定倍率資料，請先設定後再查詢',
                            labelWidth: 210,
                            margin: '0 40 0 0',
                            labelSeparator: '',
                            labelStyle: 'color: red',
                            name: 'setRatioMsg',
                            id: 'setRatioMsg'
                        },
                        {
                            xtype: 'button',
                            text: '設定單位轉換',
                            handler: function () {
                                unitWindow.show();
                            }
                        }
                    ]
                },
            ]
        },
        ]
    });

    Ext.define('T1Model', {
        extend: 'Ext.data.Model',
        fields: [
            { name: 'F01', type: 'string' },
            { name: 'F02', type: 'string' },
            { name: 'F03', type: 'string' },
            { name: 'F04', type: 'string' },
            { name: 'F05', type: 'string' },

            { name: 'F06', type: 'string' },
            { name: 'F07', type: 'string' },
            { name: 'F08', type: 'string' },
            { name: 'F09', type: 'string' },
            { name: 'F10', type: 'string' },

            { name: 'F11', type: 'string' },
            { name: 'F12', type: 'string' },
            { name: 'F13', type: 'string' },
            { name: 'F14', type: 'string' },
            { name: 'F15', type: 'string' },

            { name: 'F16', type: 'string' },
            { name: 'F17', type: 'string' },
            { name: 'F18', type: 'string' },
            { name: 'F19', type: 'string' },
            { name: 'F20', type: 'string' },

            { name: 'F21', type: 'string' },
            { name: 'F22', type: 'string' },
            { name: 'F23', type: 'string' },
            { name: 'F24', type: 'string' },
            { name: 'F25', type: 'string' },

            { name: 'F26', type: 'string' },
            { name: 'F27', type: 'string' },
            { name: 'F28', type: 'string' },
            { name: 'F29', type: 'string' },
            { name: 'F30', type: 'string' },

            { name: 'F31', type: 'string' },
            { name: 'F32', type: 'string' },
            { name: 'F33', type: 'string' },
        ]
    });

    var T1Store = Ext.create('Ext.data.Store', {
        model: 'T1Model',
        pageSize: 20, // 每頁顯示筆數
        remoteSort: true,
        sorters: [{ property: 'F03', direction: 'ASC' }, { property: 'F04', direction: 'ASC' }, { property: 'F06', direction: 'ASC' }],
        proxy: {
            type: 'ajax',
            timeout: 1800000,
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/FA0063/All',
            reader: {
                type: 'json',
                rootProperty: 'etts',
                totalProperty: 'rc'
            }
        }
        , listeners: {
            beforeload: function (store, options) {
                // 載入前將查詢條件P0值代入參數
                var np = {
                    p0: T1Query.getForm().findField('P0').getValue(),
                    p1: T1Query.getForm().findField('P1').getValue(),
                };
                Ext.apply(store.proxy.extraParams, np);
            }
        }
    });
    function T1Load() {
        T1Tool.moveFirst();
    }

    var T1Tool = Ext.create('Ext.PagingToolbar', {
        store: T1Store,
        displayInfo: true,
        border: false,
        plain: true,
        buttons: [
            {
                text: '匯出excel',
                id: 'btnExcel',
                name: 'btnExcel',
                handler: function () {
                    var param = new Array();

                    param.push({ name: 'P0', value: T1Query.getForm().findField('P0').getValue() });
                    param.push({ name: 'P1', value: T1Query.getForm().findField('P1').getValue()  });
                    PostForm('/api/FA0063/Excel', param);
                    msglabel('訊息區:匯出完成');
                }
            }, {
                text: '匯出txt',
                id: 'btnTxt',
                name: 'btnTxt',
                handler: function () {
                    var param = new Array();

                    param.push({ name: 'P0', value: T1Query.getForm().findField('P0').getValue() });
                    param.push({ name: 'P1', value: T1Query.getForm().findField('P1').getValue() });
                    PostForm('/api/FA0063/Txt', param);
                    msglabel('訊息區:匯出完成');
                }
            },
        ]
    });

    // 查詢結果資料列表
    var T1Grid = Ext.create('Ext.grid.Panel', {
        //title: T1Name,
        store: T1Store,
        id:'T1Grid',
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T1',
        dockedItems: [
            {
            dock: 'top',
            xtype: 'toolbar',
            layout: 'fit',
            items: [T1Query]
        }
            ,
            {
            dock: 'top',
            xtype: 'toolbar',
            items: [T1Tool]
            }
        ],
        columns: [{
            xtype: 'rownumberer'
        },
        {
            text: "格式",
            dataIndex: 'F01',
            width: 50
        }, {
            text: "醫事服務機構代號",
            dataIndex: 'F02',
           
        }, {
            text: "申報資料年月",
            dataIndex: 'F03',
            style: 'text-align:left',
           
        }, {
            text: "院內碼",
            dataIndex: 'F30',
            style: 'text-align:left',
        }, {
            text: "藥品代碼",
            dataIndex: 'F04',
            style: 'text-align:left',
            
        },
        {
            text: "藥商統一編號",
            dataIndex: 'F05',
            style: 'text-align:left',
           
        }, {
            text: "發票號碼（或收據號碼）",
            dataIndex: 'F06',
        }, {
            text: "發票日期",
            dataIndex: 'F07',
        }, {
            text: "發票購買藥品數量(A)",
            dataIndex: 'F08',
            style: 'text-align:left',
            width: 140, align: 'right',
        }, {
            text: "贈品數量-附贈之藥品數量(B)",
            dataIndex: 'F16',
            style: 'text-align:left',
            width: 100, align: 'right',
        }, {
            text: "贈品數量-藥品耗損數量(C)",
            dataIndex: 'F17',
            style: 'text-align:left',
            width: 100, align: 'right',
        }, {
            text: "退貨數量(D)",
            dataIndex: 'F18',
            style: 'text-align:left',
            width: 100, align: 'right',
        }, {
            text: "實際購買數量(E)",
            dataIndex: 'F19',
            style: 'text-align:left',
            width: 120, align: 'right',
        }, {
            text: "發票金額(F,元)",
            dataIndex: 'F20',
            style: 'text-align:left',
            width: 120, align: 'right',
        }, {
            text: "退貨金額(G,元)",
            dataIndex: 'F21',
            style: 'text-align:left',
            width: 120, align: 'right',
        }, {
            text: "折讓金額-折讓單金額(H,元)",
            dataIndex: 'F22',
            style: 'text-align:left',
            width: 120, align: 'right',
        }, {
            text: "折讓金額-指定捐贈(I,元)",
            dataIndex: 'F23',
            style: 'text-align:left',
            width: 120, align: 'right',
        }, {
            text: "折讓金額-藥商提撥管理費(J,元)",
            dataIndex: 'F24',
            style: 'text-align:left',
            width: 120, align: 'right',
        }, {
            text: "折讓金額-藥商提撥研究費(K,元)",
            dataIndex: 'F25',
            style: 'text-align:left',
            width: 120, align: 'right',
        },  {
            text: "折讓金額-藥商提撥補助醫師出國會議(L,元)",
            dataIndex: 'F26',
            style: 'text-align:left',
            width: 120, align: 'right',
        }, {
            text: "折讓金額-其他與本交易相關之附帶利益(M,元)",
            dataIndex: 'F27',
            style: 'text-align:left',
            width: 120, align: 'right',
        }, {
            text: "購藥總金額(N,元)",
            dataIndex: 'F28',
            style: 'text-align:left',
            width: 120, align: 'right',
        }, {
            text: "發票註記",
            dataIndex: 'F29',
            width: 120,
        }, {
            text: "廠商代碼",
            dataIndex: 'F33',
            width: 90
        }, {
            text: "訂單號碼",
            dataIndex: 'F31',
            width: 120
        }, {
            text: "進貨日期",
            dataIndex: 'F34',
            width: 120
        },{
            header: "",
            flex: 1
        }
        ],
    });

    function getHospId() {
        Ext.Ajax.request({
            url: '/api/FA0063/GetHospId',
            method: reqVal_g,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                T1Query.getForm().findField('P2').setValue(data.msg);
            },
            failure: function (response, options) {
                
            }
        });
    }
    getHospId();

    function updateHospId(hospId) {
        myMaskViewport.show();
        Ext.Ajax.request({
            url: '/api/FA0063/UpdateHospId',
            method: reqVal_p,
            params: { hospId: hospId},
            success: function (response) {
                myMaskViewport.hide();
                var data = Ext.decode(response.responseText);
                if (data.success == false) {
                    Ext.Msg.alert('錯誤', data.msg);
                    return;
                }
                msglabel('醫事服務機構代號更新成功');
                getHospId();
            },
            failure: function (response, options) {
                myMaskViewport.hide();
            }
        });
    }

    //view 
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
            items: [{
                //  xtype:'container',
                region: 'center',
                layout: {
                    type: 'border',
                    padding: 0
                },
                collapsible: false,
                title: '',
                split: true,
                width: '80%',
                flex: 1,
                minWidth: 50,
                minHeight: 140,
                items: [
                    {
                        region: 'north',
                        layout: 'fit',
                        collapsible: false,
                        title: '',
                        split: true,
                        height: '100%',
                        items: [
                            T1Grid
                        ]
                    }
                ]
            }]
        }
        ]
    });

    T1Query.getForm().findField('P0').focus();

    var myMaskViewport = new Ext.LoadMask(viewport, { msg: '處理中...' });
    myMaskViewport.hide();

    //#region editWindow
    // 查詢欄位
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
            xtype: 'container',
            layout: 'vbox',
            items: [
                {
                    xtype: 'panel',
                    border: false,
                    width: '100%',
                    layout: 'hbox',
                    items: [
                        {
                            xtype: 'checkboxfield',
                            boxLabel: '僅顯示異常',
                            name: 'starOnly',
                            id: 'starOnly',
                            style: 'margin:0px 5px 0px 5px;',
                            labelWidth: 80,
                            width: 90,
                            value: false
                        },
                        {
                            xtype: 'button',
                            text: '查詢',
                            margin: '0 0 0 20',
                            handler: function () {
                                T2Load();
                            }
                        },
                        {
                            xtype: 'button',
                            text: '重算',
                            margin: '0 0 0 20',
                            handler: function () {
                                Ext.MessageBox.confirm(
                                    "提醒",
                                    "將清空已設定倍率，是否確定重算?",
                                    function (btn) {
                                        if (btn == "yes") {
                                            Ext.Ajax.request({
                                                url: '/api/FA0063/GetNhipsurvlistInit',
                                                method: reqVal_p,
                                                params: {
                                                    reCal: 'Y'
                                                },
                                                success: function (response) {
                                                    var data = Ext.decode(response.responseText);
                                                    msglabel('倍率設定完成');
                                                    if (data.success) {
                                                        T2Load();
                                                    } else {

                                                    }
                                                },
                                                failure: function (response, options) {
                                                }
                                            });
                                        }
                                        else {
                                        }
                                    });

                                
                            }
                        },
                        {
                            xtype: 'button',
                            text: '設定單位轉換',
                            margin: '0 0 0 20',
                            handler: function () {
                                unitWindow.show();
                            }
                        }
                    ]
                }
            ]
        },
        ]
    });

    Ext.define('T2Model', {
        extend: 'Ext.data.Model',
        fields: [
            { name: 'PS_SEQ', type: 'string' },
            { name: 'M_NHIKEY', type: 'string' },
            { name: 'MMNAME_C', type: 'string' },
            { name: 'MMNAME_E', type: 'string' },
            { name: 'E_SPEC', type: 'string' },
            { name: 'E_UNIT', type: 'string' },
            { name: 'E_DRUGFORM', type: 'string' },
            { name: 'AGEN_NAME', type: 'string' },

            { name: 'BASE_UNIT', type: 'string' },
            { name: 'E_SPECINUNIT', type: 'string' },
            { name: 'E_COMPUNIT', type: 'string' },

        ]
    });
    
    var T2Store = Ext.create('Ext.data.Store', {
        model: 'T2Model',
        pageSize: 9999, // 每頁顯示筆數
        remoteSort: true,
        sorters: [{ property: 'PS_SEQ', direction: 'ASC' }],
        proxy: {
            type: 'ajax',
            timeout: 1800000,
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/FA0063/GetNhipsurvlist',
            reader: {
                type: 'json',
                rootProperty: 'etts',
                totalProperty: 'rc'
            }
        },
                listeners: {
                    beforeload: function (store, options) {
                var np = {
                    starOnly: T2Query.getForm().findField('starOnly').getValue() == true ? "Y" :"N"
                };
                Ext.apply(store.proxy.extraParams, np);
            }
        }
    });
    function T2Load() {
        T2Tool.moveFirst();
    }

    var T2Tool = Ext.create('Ext.PagingToolbar', {
        store: T2Store,
        displayInfo: true,
        border: false,
        plain: true,
        buttons: [
            {
                text: '儲存',
                name: 'savePage',
                handler: function () {
                    var data = [];
                    var tempData = T2Grid.getStore().data.items;

                    for (var i = 0; i < tempData.length; i++) {
                        if (tempData[i].dirty) {
                            //if (tempData[i].data.CHK_QTY == '' || tempData[i].data.CHK_QTY == null) {
                            //    var msg = '';
                            //    msg = tempData[i].data.MMCODE + ' 請輸入盤點量'
                            //    Ext.Msg.alert('提示', msg);
                            //    return;
                            //}

                            data.push(tempData[i].data);
                        }
                    }

                    myMaskViewport.show();
                    Ext.Ajax.request({
                        url: '/api/FA0063/UpdateNhupsurvlistSetRatio',
                        method: reqVal_p,
                        contentType: "application/json",
                        params: {
                            itemString: Ext.util.JSON.encode(data)
                        },
                        success: function (response) {

                            myMaskViewport.hide();
                            var data = JSON.parse(response.responseText);
                            if (data.success == false) {
                                Ext.Msg.alert('失敗', data.msg);
                                return;
                            }

                            msglabel('訊息區:資料更新成功');
                            T2Store.load({
                                params: {
                                    start: 0
                                }
                            });
                        },

                        failure: function (response, action) {
                            myMaskViewport.hide();
                            Ext.Msg.alert('失敗', 'Ajax communication failed');
                        }
                    });
                }
            },
        ]
    });

    var T2Grid = Ext.create('Ext.grid.Panel', {
        store: T2Store,
        id: 'T2Grid',
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T1',
        dockedItems: [
            {
                dock: 'top',
                xtype: 'toolbar',
                layout: 'fit',
                items: [T2Query]
            }
            ,
            {
                dock: 'top',
                xtype: 'toolbar',
                items: [T2Tool]
            }
        ],
        columns: [
        //    {
        //    xtype: 'rownumberer',
        //    width:50
        //},
        {
            text: "序號",
            dataIndex: 'PS_SEQ',
            style: 'text-align:left',
            align: 'right',
            width: 50
        }, {
            text: "健保碼",
            dataIndex: 'M_NHIKEY',

        }, {
            text: "中文品名",
            dataIndex: 'MMNAME_C',

        }, {
            text: "英文品名",
            dataIndex: 'MMNAME_E',

        },
        {
            text: "NHI_規格量",
            style: 'text-align:left',
            align: 'right',
            dataIndex: 'E_SPEC',
            width: 80,

        }, {
            text: "NHI_規格單位",
            dataIndex: 'E_UNIT',
        }, {
            text: "NHI_藥品劑型",
            dataIndex: 'E_DRUGFORM',
        }, {
            text: "院內碼",
            dataIndex: 'MMCODE',
            width: 70,

        }, {
            text: "HIS_計量單位",
            dataIndex: 'HIS_BASE_UNIT',

        }, {
            text: "HIS_規格量及單位",
            dataIndex: 'HIS_E_SPECNUNIT',
        }, {
            text: "HIS_成份",
            dataIndex: 'HIS_E_COMPUNIT',
        }, {
            text: "HIS_藥品劑型",
            dataIndex: 'HIS_E_DRUGFORM',
        }, {
            text: "建議倍率",
            dataIndex: 'RCM_RATIO',
            width:70,
            style: 'text-align:left',
            align: 'right',
        }, {
            text: "設定倍率",
            dataIndex: 'SET_RATIO',
            width: 70,
            align: 'right',
            style: 'text-align:left; color:red',
            editor: {
                xtype: 'textfield',
                regexText: '只能輸入數字',
                regex: /^[0-9]+$/, // 用正規表示式限制可輸入內容
                selectOnFocus: true,
                listeners: {
                    change: function (field, newVal, oldVal) {
                    },
                }
            },
        },  {
            header: "",
            flex: 1
        }
        ],
        plugins: [
            Ext.create('Ext.grid.plugin.CellEditing', {
                clicksToEdit: 1
            })
        ],
        listeners: {
            beforeedit: function (editor, e) {
                var editColumnIndex = findColumnIndex(T2Grid.columns, 'SET_RATIO');
                // STATUS_INI不是1 則不可填寫
                if (e.colIdx != editColumnIndex) {
                    return false;
                }
            }
        },
    });

    var findColumnIndex = function (columns, dataIndex) {
        var index;
        for (index = 0; index < columns.length; ++index) {
            if (columns[index].dataIndex == dataIndex) { break; }
        }
        return index == columns.length ? -1 : index;
    }

    var editWindow = Ext.create('Ext.window.Window', {
        renderTo: Ext.getBody(),
        modal: true,
        items: [T2Grid],
        width: windowWidth,
        height: windowHeight,
        xtype: 'form',
        layout: 'form',
        resizable: false,
        draggable: false,
        closable: false,
        layout: {
            type: 'fit',
            padding: 0
        },
        title: "轉換倍率設定",
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
                    checkSetRatioEmpty();
                    getLastUploadtTime();
                    T2Query.getForm().findField('starOnly').setValue(false);
                    editWindow.hide();
                }
            }
        ]
    });
    editWindow.hide();
    //#endregion

    //#region unitindow
    // 查詢欄位
    var T3Query = Ext.widget({
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
            xtype: 'container',
            layout: 'vbox',
            items: [
                {
                    xtype: 'panel',
                    border: false,
                    width: '100%',
                    layout: 'hbox',
                    items: [
                        {
                            xtype: 'textfield',
                            fieldLabel: '計量單位轉換前',
                            name: 'T3_P0',
                            id: 'T3_P0',
                            labelWidth: 90,
                            width: 180
                        },
                        {
                            xtype: 'button',
                            text: '查詢',
                            margin: '0 0 0 20',
                            handler: function () {
                                T3Load();
                            }
                        },
                        {
                            xtype: 'button',
                            text: '清除',
                            margin: '0 5 0 5',
                            handler: function () {
                                var f = this.up('form').getForm();
                                f.reset();
                                getHospId();
                                f.findField('T3_P0').focus(); 
                            }
                        },
                    ]
                }
            ]
        },
        ]
    });

    Ext.define('T3Model', {
        extend: 'Ext.data.Model',
        fields: [
            { name: 'UI_FROM', type: 'string' },
            { name: 'UI_TO', type: 'string' },
            { name: 'COEFF_FROM', type: 'string' },
            { name: 'COEFF_TO', type: 'string' },
            { name: 'CNVNOTE', type: 'string' }
        ]
    });

    var T3Store = Ext.create('Ext.data.Store', {
        model: 'T3Model',
        pageSize: 20, // 每頁顯示筆數
        remoteSort: true,
        sorters: [{ property: 'UI_FROM', direction: 'ASC' }],
        proxy: {
            type: 'ajax',
            timeout: 1800000,
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/FA0063/GetBaseUnitCnvs',
            reader: {
                type: 'json',
                rootProperty: 'etts',
                totalProperty: 'rc'
            }
        },
        listeners: {
            beforeload: function (store, options) {
                var np = {
                    ui_from: T3Query.getForm().findField('T3_P0').getValue()
                };
                Ext.apply(store.proxy.extraParams, np);
            }
        }
    });
    function T3Load() {
        T3Tool.moveFirst();
    }
    var T3Set = '';
    var T3Tool = Ext.create('Ext.PagingToolbar', {
        store: T3Store,
        displayInfo: true,
        border: false,
        plain: true,
        buttons: [
            {
                itemId: 'add', text: '新增', handler: function () {
                    T3Set = '/api/FA0063/CreateBaseunitcnv';
                    msglabel('訊息區:');
                    setFormT3('I', '新增');
                }
            },
            {
                itemId: 'edit', text: '修改', disabled: true, handler: function () {
                    T3Set = '/api/FA0063/UpdateBaseunitcnv';
                    msglabel('訊息區:');
                    setFormT3("U", '修改');
                }
            }
            , {
                itemId: 'delete', text: '刪除', disabled: true,
                handler: function () {
                    Ext.MessageBox.confirm('刪除', '是否確定刪除資料?<br>' + name, function (btn, text) {
                        if (btn === 'yes') {
                            Ext.Ajax.request({
                                url: '/api/FA0063/DeleteBaseunitcnv',
                                method: reqVal_p,
                                params: {
                                    UI_FROM: T3LastRec.data.UI_FROM,
                                    UI_TO: T3LastRec.data.UI_TO
                                },
                                success: function (response) {
                                    var data = Ext.decode(response.responseText);
                                    if (data.success) {
                                        T3Load();
                                        msglabel('資料刪除成功');
                                    }
                                    else
                                        Ext.MessageBox.alert('錯誤', data.msg);
                                },
                                failure: function (response) {
                                    Ext.MessageBox.alert('錯誤', '發生例外錯誤');
                                }
                            });
                        }
                    }
                    );
                }
            },
        ]
    });

    var T3Grid = Ext.create('Ext.grid.Panel', {
        store: T3Store,
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T1',
        dockedItems: [
            {
                dock: 'top',
                xtype: 'toolbar',
                layout: 'fit',
                items: [T3Query]
            }
            ,
            {
                dock: 'top',
                xtype: 'toolbar',
                items: [T3Tool]
            }
        ],
        columns: [
                {
                xtype: 'rownumberer',
                width:50
            },
            {
                text: "計量單位轉換前",
                dataIndex: 'UI_FROM',
                width: 70
            }, {
                text: "計量單位轉換後",
                dataIndex: 'UI_TO',
                width: 70

            }, {
                text: "係數轉換前",
                dataIndex: 'COEFF_FROM',
                width: 70

            }, {
                text: "係數轉換後",
                dataIndex: 'COEFF_TO',
                width: 70

            },
            {
                text: "說明",
                dataIndex: 'CNVNOTE',
                width: 80,

            },{
                header: "",
                flex: 1
            }
        ],
        listeners: {
            selectionchange: function (model, records) {
                T3Rec = records.length;
                T3LastRec = records[0];
                setFormT3a();
            }
        }
    });
    var T3Form = Ext.widget({
        xtype: 'form',
        layout: 'form',
        frame: false,
        cls: 'T1b',
        title: '',
        autoScroll: true,
        bodyPadding: '5 5 0',
        fieldDefaults: {
            labelAlign: 'right',
            msgTarget: 'side',
            labelWidth: 90
        },
        height: '100%',
        defaultType: 'textfield',
        items: [
            {
                name: 'x',
                xtype: 'hidden'
            }, {
                fieldLabel: '計量單位轉換前',
                name: 'UI_FROM',
                enforceMaxLength: true,
                maxLength: 40,
                readOnly: true
            }, {
                fieldLabel: '計量單位轉換後',
                name: 'UI_TO',
                enforceMaxLength: true,
                maxLength: 40,
                readOnly: true
            }, {
                xtype:'numberfield',
                fieldLabel: '係數轉換前',
                name: 'COEFF_FROM',
                enforceMaxLength: true,
                maxLength: 40,
                minValue: 0,
                hideTrigger: true,
                allowBlank: false,
                decimalPrecision:9, 
                readOnly: true
            }, {
                xtype: 'numberfield',
                fieldLabel: '係數轉換後',
                name: 'COEFF_TO',
                enforceMaxLength: true,
                maxLength: 40,
                minValue: 0,
                hideTrigger:true,
                allowBlank: false,
                decimalPrecision: 9, 
                readOnly: true
            }, {
                fieldLabel: '說明',
                name: 'CNVNOTE',
                enforceMaxLength: true,
                maxLength: 40,
                readOnly: true
            }
        ],
        buttons: [{
            itemId: 'submit', text: '儲存', hidden: true,
            handler: function () {
                if (this.up('form').getForm().isValid()) { // 檢查T1Form填寫資料是否符合規則(必填欄位都有填、輸入內容有符合正規表示式等)

                    var x = T3Form.getForm().findField('x').getValue();
                    confirmSubmit = '';
                    if (x == 'I') {
                        confirmSubmit = '新增';
                    } else if (x == 'U') {
                        confirmSubmit = '修改';
                    }

                    Ext.MessageBox.confirm(confirmSubmit, '是否確定' + confirmSubmit + '?', function (btn, text) {
                        if (btn === 'yes') {
                            T3Submit();
                        }
                    }
                    );
                }
                else
                    Ext.Msg.alert('提醒', '輸入資料格式有誤');
            }
        }, {
            itemId: 'cancel', text: '取消', hidden: true, handler: T3Cleanup
        }]
    });
    function T3Submit() {
        var f = T3Form.getForm();
        if (f.isValid()) {
            var myMask = new Ext.LoadMask(T3Grid, { msg: '處理中...' });
            myMask.show();
            f.submit({
                url: T3Set,
                success: function (form, action) {
                    myMask.hide();
                    var f2 = T3Form.getForm();
                    var r = f2.getRecord();
                    switch (f2.findField("x").getValue()) {
                        case "I":
                            msglabel('訊息區:資料新增成功');
                            break;
                        case "U":
                            msglabel('訊息區:資料修改成功');
                            break;
                        //case "D":
                        //    T1Store.remove(r); // 若刪除後資料需從查詢結果移除可用remove
                        //    r.commit();
                        //    break;
                    }
                    
                    T3Query.getForm().findField('T3_P0').setValue(f2.findField('UI_FROM').getValue());
                    T3Load();
                    T3Cleanup();
                    
                    
                },
                failure: function (form, action) {
                    myMask.hide();
                    switch (action.failureType) {
                        case Ext.form.action.Action.CLIENT_INVALID:
                            Ext.Msg.alert('失敗', 'Form fields may not be submitted with invalid values');
                            break;
                        case Ext.form.action.Action.CONNECT_FAILURE:
                            Ext.Msg.alert('失敗', 'Ajax communication failed');
                            break;
                        case Ext.form.action.Action.SERVER_INVALID:
                            Ext.Msg.alert('失敗', action.result.msg);
                            break;
                    }
                }
            });
        }
    }
    function T3Cleanup() {
        T3Grid.unmask();
        var f = T3Form.getForm();
        f.reset();
        f.getFields().each(function (fc) {
            if (fc.xtype == "displayfield" || fc.xtype == "textfield" || fc.xtype == "numberfield") {
                fc.setReadOnly(true);
            } else if (fc.xtype == "combo" || fc.xtype == "datefield") {
                fc.readOnly = true;
            }
        });
        T3Form.down('#cancel').hide();
        T3Form.down('#submit').hide();
        //viewport.down('#form').setTitle('瀏覽');
        setFormT3a();
    }
    function setFormT3(x, t) {
        T3Grid.mask();
        var f = T3Form.getForm();
        var u = f.findField('UI_FROM');
        if (x === "I") {
            isNew = true;
            var r = Ext.create('T3Model'); // /Scripts/app/model/MI_WHMAST.js
            T3Form.loadRecord(r); // 建立空白model,在新增時載入T1Form以清空欄位內容
            u.setReadOnly(false);
            f.findField('UI_TO').setReadOnly(false);
        }

        f.findField('x').setValue(x);
        f.findField('COEFF_FROM').setReadOnly(false);
        f.findField('COEFF_TO').setReadOnly(false);
        f.findField('CNVNOTE').setReadOnly(false);

        T3Form.down('#cancel').setVisible(true);
        T3Form.down('#submit').setVisible(true);
        u.focus();
    }
    function setFormT3a() {
        T3Grid.down('#edit').setDisabled(T3Rec === 0);
        T3Grid.down('#delete').setDisabled(T3Rec === 0);
        
        if (T3LastRec) {
            isNew = false;
            T3Form.loadRecord(T3LastRec);
            var f = T3Form.getForm();
            f.findField('x').setValue('U');
            var u = f.findField('UI_FROM');
            u.setReadOnly(true);
            u.setFieldStyle('border: 0px');

            u = f.findField('UI_TO');
            u.setReadOnly(true);
            u.setFieldStyle('border: 0px');

        }
        else {
            T3Form.getForm().reset();
        }
    }

    var unitWindow = Ext.create('Ext.window.Window', {
        renderTo: Ext.getBody(),
        modal: true,
        items: [{
            xtype: 'container',
            //layout: 'fit',
            layout: 'hbox',
            width: '100%',
            
            items: [
               // T3Grid, T3Form
                {
                    xtype: 'panel',
                layout: 'fit',
                collapsible: false,
                title: '',
                border: false,
                items: [T3Grid],
                width: '70%',
                height: '100%',
                }
                , {
                    xtype: 'panel',
                layout: 'fit',
                collapsible: false,
                title: '',
                border: false,
                items: [T3Form],
                width: '30%',
                height: '100%',
                },
            ]
        }
            
       ],
        width: windowWidth-200,
        height: windowHeight,
        xtype: 'form',
        layout: 'form',
        resizable: false,
        draggable: false,
        closable: false,
        layout: {
            type: 'fit',
            padding: 0
        },
        title: "單位轉換設定",
        buttons: [
            {
                text: '關閉',
                handler: function () {
                    unitWindow.hide();
                }
            }
        ],
        listeners: {
            show: function () {
                T3Load();
                unitWindow.center();
            }
        }
    });
    unitWindow.hide();
    //#endregion

    Ext.on('resize', function () {
        windowHeight = $(window).height();
        windowWidth = $(window).width();
        editWindow.setHeight(windowHeight);
        editWindow.setWidth(windowWidth);

        unitWindow.setHeight(windowHeight);
        unitWindow.setWidth(windowWidth-200);
    });
});
