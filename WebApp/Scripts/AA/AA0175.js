Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});
Ext.require(['WEBAPP.utils.Common']);

Ext.onReady(function () {
    var T1Set = '';
    var T1Name = "儲位管理";
    var windowHeight = $(window).height();
    var windowWidth = $(window).width();
    var T1LastRec = null;

    var st_matclasssub = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT']
    });
    var T2Store = Ext.create('Ext.data.Store', {
        fields: ['WH_NO', 'MMCODE', 'STORE_LOC', 'SaveStatus', 'UploadMsg', 'INV_QTY', 'LOC_NOTE']
    });

    Ext.override(Ext.grid.column.Column, { menuDisabled: true });

    Ext.getUrlParam = function (param) {
        var params = Ext.urlDecode(location.search.substring(1));
        return param ? params[param] : params;
    };
    var menuLink = Ext.getUrlParam('menuLink');

    var T1QueryAgenNoCombo = Ext.create('WEBAPP.form.AgenNoCombo_2', {
        name: 'P0',
        id: 'P0',
        fieldLabel: '廠商代碼',
        labelAlign: 'right',
        //fieldCls: 'required',
        //allowBlank: false,
        labelWidth: 100,
        width: 200,
        emptyText: '全部',
        limit: 1000,//限制一次最多顯示10筆
        queryUrl: '/api/AA0175/GetAgenNoCombo',//指定查詢的Controller路徑
        extraFields: ['AGEN_NO', 'AGEN_NAMEC'],//查詢完會回傳的欄位
        getDefaultParams: function () {//查詢時Controller固定會收到的參數

        },
        listeners: {
            select: function (c, r, i, e) {
            }
        }
    });

    var T1QuerymmCodeCombo = Ext.create('WEBAPP.form.MMCodeCombo', {
        id: 'P1',
        name: 'P1',
        fieldLabel: '藥材代碼',
        labelWidth: 100,
        width: 220,
        emptyText: '全部',
        //allowBlank: true,
        limit: 5000,
        queryUrl: '/api/AA0175/GetMMCodeCombo',
        //查詢完會回傳的欄位
        extraFields: ['MMCODE', 'MMNAME_C', 'MMNAME_E'],
        getDefaultParams: function () {

        },
        listeners: {
        }
    });

    var T1FormMMCode = Ext.create('WEBAPP.form.MMCodeCombo', {
        name: 'MMCODE',
        fieldLabel: '院內碼',
        labelAlign: 'right',
        limit: 10, //限制一次最多顯示10筆
        queryUrl: '/api/AA0175/GetMMCodeCombo', //指定查詢的Controller路徑
        extraFields: ['MMNAME_C', 'MMNAME_E', 'BASE_UNIT'], //查詢完會回傳的欄位
        getDefaultParams: function () { //查詢時Controller固定會收到的參數
            
        },
        listeners: {
            select: function (c, r, i, e) {
                //選取下拉項目時，顯示回傳值
                
                T1Form.getForm().findField('F3').setValue(r.get('MMNAME_C'));
            }
        }
    });

    function setComboData() {

        //P2 藥材類別
        Ext.Ajax.request({
            url: '/api/AA0175/GetMatClassSubCombo',
            method: reqVal_g,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    var matclasssubs = data.etts;
                    if (matclasssubs.length > 0) {
                        for (var i = 0; i < matclasssubs.length; i++) {
                            st_matclasssub.add({ VALUE: matclasssubs[i].VALUE, TEXT: matclasssubs[i].TEXT });
                        }
                    }
                }
            },
            failure: function (response, options) {

            }
        });
    }
    setComboData();

    //庫房代碼
    var T1QueryWhnoCombo = Ext.create('Ext.data.Store', {
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AA0175/GetWhnoCombo',
            reader: {
                type: 'json',
                rootProperty: 'etts'
            }
        },
        autoLoad: true
    });

    // 查詢欄位
    var mLabelWidth = 70;
    var mWidth = 230;
    var T1Query = Ext.widget({
        xtype: 'form',
        layout: 'form',
        border: false,
        //autoScroll: true, // 若為true,容器內容超出容器大小時出現捲動條;若為false超出容器的部分會被裁切掉
        fieldDefaults: {
            xtype: 'textfield',
            labelAlign: 'right',
            labelWidth: mLabelWidth,
            width: mWidth
        },
        items: [{
            xtype: 'container',
            layout: 'hbox',
            items: [
                {
                    xtype: 'panel',
                    id: 'PanelP1',
                    border: false,
                    layout: 'hbox',
                    items: [
                        T1QueryAgenNoCombo,
                        T1QuerymmCodeCombo,
                        {
                            xtype: 'combo',
                            fieldLabel: '藥材類別',
                            name: 'P2',
                            id: 'P2',
                            labelWidth: 100,
                            width: 200,
                            emptyText: '全部',
                            store: st_matclasssub,
                            queryMode: 'local',
                            displayField: 'TEXT',
                            valueField: 'VALUE'
                        },
                        {
                            xtype: 'combo',
                            store: T1QueryWhnoCombo,
                            fieldLabel: '庫房代碼',
                            name: 'P3',
                            id: 'P3',
                            labelWidth: 100,
                            width: 220,
                            emptyText: '全部',
                            queryMode: 'local',
                            labelAlign: 'right',
                            displayField: 'TEXT',
                            valueField: 'VALUE',
                            tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{VALUE}&nbsp;{TEXT}</div></tpl>',
                        }, {
                            name: 'P4',
                            xtype: 'hidden'
                        }, {
                            fieldLabel: '無儲位',
                            name: 'P04',
                            xtype: 'checkboxfield',
                            labelWidth: 80,
                            labelAlign: 'right',
                            listeners: {
                                change: function (checkbox, newValue, oldValue, eOpts) {
                                    if (checkbox.checked) {
                                        T1Query.getForm().findField('P4').setValue("Y");
                                    } else {
                                        T1Query.getForm().findField('P4').setValue("N");
                                    }
                                }
                            }
                        },
                        {
                            xtype: 'tbspacer',
                            width:50
                        },
                        {
                            xtype: 'button',
                            text: '查詢',
                            handler: function () {
                                T1Load();
                                //var MAT_CLASS = T1Query.getForm().findField('P0').getValue();
                                //var tempWH_NO = T1Query.getForm().findField('WH_NO').getValue();
                                //var TXTDay_B = T1Query.getForm().findField('P1').getValue();
                                //var TXTDay_E = T1Query.getForm().findField('P2').getValue();

                                //if (MAT_CLASS != null && MAT_CLASS != '' && tempWH_NO != null && tempWH_NO != '' && TXTDay_B != null && TXTDay_B != '' && TXTDay_E != null && TXTDay_E != '') {
                                //    WH_NO = tempWH_NO;
                                //    msglabel('訊息區:');
                                //    T1Load();
                                //    Ext.getCmp('btnUpdate').setDisabled(true);
                                //    Ext.getCmp('btnDel').setDisabled(true);
                                //    Ext.getCmp('btnTrans').setDisabled(true);

                                //}
                                //else {
                                //    Ext.MessageBox.alert('提示', '請輸入必填/必選資料後再進行查詢');
                                //}
                                //Ext.getCmp('eastform').collapse();
                            }
                        },
                        {
                            xtype: 'button',
                            text: '清除',
                            handler: function () {
                                var f = this.up('form').getForm();
                                f.reset();
                                f.findField('P0').focus(); // 進入畫面時輸入游標預設在P0
                                T1Query.getForm().findField('P4').setValue('N');
                            }
                        }]
                }
            ]
        }]
    });

    Ext.define('T1Model', {
        extend: 'Ext.data.Model',
        fields: [
            { name: 'F1', type: 'string' },
            { name: 'F2', type: 'string' },
            { name: 'F3', type: 'string' },
            { name: 'F4', type: 'string' },
            { name: 'F5', type: 'string' },
            { name: 'F6', type: 'string' },
            { name: 'F7', type: 'string' },
            { name: 'F8', type: 'string' },
            { name: 'F9', type: 'string' }
        ]
    });
    var T1Store = Ext.create('Ext.data.Store', {
        model: 'T1Model',
        pageSize: 30, // 每頁顯示筆數
        remoteSort: true,
        sorters: [{ property: 'F1', direction: 'ASC' }, { property: 'F2', direction: 'ASC' }],
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AA0175/All',
            reader: {
                type: 'json',
                rootProperty: 'etts',
                totalProperty: 'rc'
            }
        },
        listeners: {
            beforeload: function (store, options) {
                // 載入前將查詢條件P0~P4的值代入參數
                var np = {
                    p0: T1Query.getForm().findField('P0').getValue(),
                    p1: T1Query.getForm().findField('P1').getValue(),
                    p2: T1Query.getForm().findField('P2').getValue(),
                    p3: T1Query.getForm().findField('P3').getValue(),
                    p4: T1Query.getForm().findField('P4').getValue()
                };
                Ext.apply(store.proxy.extraParams, np);
            },
            load: function (store, options) {
                //載入資料>0筆就可以匯出
                var dataCount = store.getCount();
                if (dataCount > 0) {
                    Ext.getCmp('btnExport').setDisabled(false);
                } else {
                    Ext.getCmp('btnExport').setDisabled(true);
                }
            }
        }
    });

    function getBtnExportDefaultHidden(menuLink) {
        if (["AA0203", "AA0207"].includes(menuLink)) {
            return true;
        } else {
            return false;
        }
    }

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
                text: '新增',
                id: 'btnAdd',
                name: 'btnAdd',
               // disabled: true,
                handler: function () {
                    T1Set = '/api/AA0175/MasterCreate';
                    setFormT1('I', '新增');
                }
            },
            {
                text: '修改',
                id: 'btnUpdate',
                name: 'btnUpdate',
                disabled: true,
                handler: function () {
                    T1Set = '/api/AA0175/MasterUpdate';
                    setFormT1('U', '修改');
                }
            },
            {
                text: '刪除',
                id: 'btnDel',
                name: 'btnDel',
                disabled: true,
                handler: function () {
                    var selection = T1Grid.getSelection();
                    var selectItem = '';
                    if (selection.length) {
                        //selection.map(item => {
                        //    pr_no = item.get('PR_NO');
                        //});

                        $.map(selection, function (item, key) {
                            selectItem += item.get('WH_NO') + ',' + item.get('F2') + ',' + item.get('F4') + "<br>";
                        })

                        Ext.MessageBox.confirm('刪除', '是否確定刪除選取資料?<br>', function (btn, text) {
                            if (btn === 'yes') {
                                Ext.Ajax.request({
                                    url: '/api/AA0175/MasterDelete',
                                    method: reqVal_p,
                                    params: {
                                        item: selectItem
                                    },
                                    success: function (response) {
                                        var data = Ext.decode(response.responseText);
                                        if (data.success) {
                                            msglabel('訊息區:資料刪除成功');

                                            T1Load();
                                        }
                                        else
                                            Ext.MessageBox.alert('錯誤', data.msg);
                                    },
                                    failure: function (response) {
                                        Ext.MessageBox.alert('錯誤', '發生例外錯誤');
                                    }
                                });
                            }
                        });
                    }
                }
            },
            {
                id: 'btnExport',
                itemId: 'btnExport',
                text: '匯出',
                disabled: true,
                hidden: getBtnExportDefaultHidden(menuLink),
                handler: function () {
                    var p = new Array();
                    p.push({ name: 'FN', value: '藥材儲位清單.xls' });
                    p.push({ name: 'p0', value: T1Query.getForm().findField('P0').getValue() });
                    p.push({ name: 'p1', value: T1Query.getForm().findField('P1').getValue() });
                    p.push({ name: 'p2', value: T1Query.getForm().findField('P2').getValue() });
                    p.push({ name: 'p3', value: T1Query.getForm().findField('P3').getValue() });
                    p.push({ name: 'p4', value: T1Query.getForm().findField('P4').getValue() });

                    PostForm('/api/AA0175/Excel', p);
                    msglabel('匯出完成');
                }
            },
            {
                xtype: 'filefield',
                name: 'uploadExcel',
                id: 'uploadExcel',
                buttonText: '匯入',
                buttonOnly: true,
                padding: '0 4 0 0',
                width: 72,
                listeners: {
                    change: function (widget, value, eOpts) {
                        var files = event.target.files; //取得檔案訊息
                        if (!files || files.length == 0) return; // make sure we got something
                        var file = files[0];
                        var ext = this.value.split('.').pop();//取得副檔名
                        if (!/^(xls|xlsx|XLS|XLSX)$/.test(ext)) {
                            Ext.MessageBox.alert('提示', '僅支持讀取xlsx和xls格式！');
                            Ext.getCmp('import').fileInputEl.dom.value = '';
                            msglabel('');
                        } else {
                            uploadCheckYWindow.show();
                            //msglabel("已選擇檔案");
                            var myMask = new Ext.LoadMask(viewport, { msg: '處理中...' });
                            myMask.show();
                            var formData = new FormData();
                            formData.append("file", file);
                            var ajaxRequest = $.ajax({
                                type: "POST",
                                url: "/api/AA0175/Upload",
                                data: formData,
                                processData: false,
                                //必須false才會自動加上正確的Content-Type
                                contentType: false,
                                success: function (data, textStatus, jqXHR) {
                                    myMask.hide();
                                    if (!data.success) {
                                        Ext.MessageBox.alert("提示", data.msg);
                                        msglabel("訊息區:");
                                        Ext.getCmp('uploadExcel').fileInputEl.dom.value = '';
                                    }
                                    else {
                                        msglabel("訊息區:資料匯入成功");
                                        Ext.getCmp('uploadExcel').fileInputEl.dom.value = '';

                                        //T1Load();
                                        setT2Store(data.etts);
                                    }
                                },
                                error: function (jqXHR, textStatus, errorThrown) {
                                    myMask.hide();
                                    Ext.Msg.alert('失敗', 'Ajax communication failed');
                                    //T1Query.getForm().findField('send').fileInputEl.dom.value = '';
                                    Ext.getCmp('uploadExcel').setRawValue("");
                                }
                            });
                        }
                    }
                }
            }
        ]
    });

    var T2Grid = Ext.create('Ext.grid.Panel', {
        store: T2Store, //資料load進來
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T2',
        id: 't2Grid',
        columns: [{
            text: "院內碼",
            dataIndex: 'MMCODE',
            width: 80
        }, {
            text: "是否通過",
            dataIndex: 'SaveStatus',
            width: 80,
            renderer: function (val, meta, record) {
                if (record.data.SaveStatus == 'Y') {
                    return '通過';
                }
                if (record.data.SaveStatus == 'N') {
                    return '不通過';
                }
            },
        }, {
            text: "原因",
            dataIndex: 'UploadMsg',
            width: 120,
            renderer: function (value, meta, record) {
                meta.style = 'white-space:normal;word-break:break-all;';
                return value;
            }
        }, {
            text: "儲位位置",
            dataIndex: 'STORE_LOC',
            width: 150
        //}, {
        //    text: "儲位數量",
        //    dataIndex: 'INV_QTY',
        //    width: 150
        }, {
            text: "備註",
            dataIndex: 'LOC_NOTE',
            width: 120
        }, {
            header: "",
            flex: 1
        }],
    });

    var uploadCheckYWindow = Ext.create('Ext.window.Window', {
        renderTo: Ext.getBody(),
        id: 'uploadCheckYWindow',
        width: windowWidth,
        height: windowHeight,
        xtype: 'form',
        layout: 'form',
        modal: true, //true 掩閉背后的一切
        title: '本次修改項目(有填欄位才會更新)',
        closable: false,
        layout: {
            type: 'fit',
            padding: 0
        },
        defaults: {
            split: true  //可以調整大小
        },
        items: [{
            itemId: 't2Grid',
            region: 'center',
            layout: 'fit',
            split: true,
            collapsible: false,
            border: false,
            items: [T2Grid]
        }],
        listeners: {
            show: function () {
                uploadCheckYWindow.center();
            }
        },
        buttons: [
            {
                id: 'uploadConfirmY',
                hidden: true,
                text: '確定上傳',
                handler: function () {
                    
                    var temp_datas = T2Store.getData().getRange();
                    var list = [];
                    for (var i = 0; i < temp_datas.length; i++) {
                        list.push(temp_datas[i].data);
                    }

                    Ext.Ajax.request({
                        url: '/api/AA0175/UploadConfirm',
                        method: reqVal_p,
                        params: { data: Ext.util.JSON.encode(list), isYear: 'Y' },
                        success: function (response) {
                            var data = Ext.decode(response.responseText);
                            if (data.success) {
                                msglabel('資料上傳成功');
                                uploadCheckYWindow.hide();
                                T1Load(true);
                            } else {
                                msglabel('資料上傳失敗');
                                Ext.Msg.alert('提醒', data.msg);
                            }
                        },
                        failure: function (response, options) {

                        }
                    });
                }
            },
            {
                id: 'closeUploadCheckYWindow',
                disabled: false,
                text: '關閉',
                handler: function () {
                    uploadCheckYWindow.hide();
                }
            }
        ]
    });

    //uploadCheckYWindow 匯入時檢查資料是否全符合，彈出視窗顯示結果
    function setT2Store(datas) {
        
        T2Store.removeAll();
        var showConfirm = true;
        for (var i = 0; i < datas.length; i++) {
            T2Store.add(datas[i]);
            if (datas[i].SaveStatus == 'N') {
                showConfirm = false;
            }
        }

        if (showConfirm == false) {
            Ext.getCmp('uploadConfirmY').hide();
        } else {
            Ext.getCmp('uploadConfirmY').show();
        }
    }

    function T1ChkBtns() {
        var selection = T1Grid.getSelection();
        if (selection.length) {
            if (selection.length == 1) {
                Ext.getCmp('btnUpdate').setDisabled(false);
                Ext.getCmp('btnDel').setDisabled(false);
                Ext.getCmp('btnAdd').setDisabled(false);
            }
        }
        else {
            Ext.getCmp('btnUpdate').setDisabled(true);
            Ext.getCmp('btnDel').setDisabled(true);
            Ext.getCmp('btnAdd').setDisabled(true);
        }
    }

    var checkboxT1Model = Ext.create('Ext.selection.CheckboxModel', {
        listeners: {
            'beforeselect': function (view, rec, index) {
                //if (rec.get('PR_STATUS') != '35') {
                //    // Ext.Msg.alert('訊息', rec.get('PR_NO') + '狀態不為申購單開立');
                //    return false;
                //}
            },
            'select': function (view, rec) {
                //if (rec.get('PR_STATUS') == '35')
                //    selRecords.insertarray(rec.get('PR_NO'));
                T1ChkBtns();
            },
            'deselect': function (view, rec) {
                //selRecords.deletearray(rec.get('PR_NO'));
                T1ChkBtns();
            }
        }
    });

    // 點選T1Grid一筆資料的動作
    function setFormT1a() {
        
        if (T1LastRec != null) {
            viewport.down('#form').expand();
            var f = T1Form.getForm();
            
            f.findField("WH_NO").setValue(T1LastRec.data["WH_NO"]);
            f.findField("MMCODE").setValue(T1LastRec.data["F2"]);
            f.findField("MMCODE_TEXT").setValue(T1LastRec.data["F2"]);
            f.findField("F3").setValue(T1LastRec.data["F3"]);
            f.findField("STORE_LOC").setValue(T1LastRec.data["F4"]);
            f.findField("STORE_LOC_TEXT").setValue(T1LastRec.data["F4"]);
          //  f.findField("INV_QTY").setValue(T1LastRec.data["F5"]);
          //  f.findField("INV_QTY_TEXT").setValue(T1LastRec.data["F5"]);
            f.findField("F6").setValue(T1LastRec.data["F6"]);
            f.findField("LOC_NOTE").setValue(T1LastRec.data["F7"]);
            f.findField("LOC_NOTE_TEXT").setValue(T1LastRec.data["F7"]);
          //  f.findField("F8").setValue(T1LastRec.data["F8"]);
          //  f.findField("F9").setValue(T1LastRec.data["F9"]);
            f.findField("ORI_STORE_LOC").setValue(T1LastRec.data["ORI_STORE_LOC"]);

            T1Form.down('#cancel').setVisible(false);
            T1Form.down('#submit').setVisible(false);
        }

    }

    var T1Grid = Ext.create('Ext.grid.Panel', {
        //title: T1Name,
        store: T1Store,
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
            },
            {
                dock: 'top',
                xtype: 'toolbar',
                items: [T1Tool]
            }
        ],
        selModel: checkboxT1Model,
        columns: [
            {
                xtype: 'rownumberer',
            },
            {
                text: "庫房代碼",
                dataIndex: 'F1',
                width: 100
            },
            {
                text: "藥材代碼",
                dataIndex: 'F2',
                width: 100
            },
            {
                text: "藥材名稱",
                dataIndex: 'F3',
                width: 350,
                //renderer: function (value) {
                //    var matclassTxt = '';
                //    MATQueryStore.each(function (rec) {
                //        if (value == rec.get('VALUE')) {
                //            matclassTxt = rec.get('TEXT');
                //            return false;
                //        };
                //    });
                //    return matclassTxt;
                //}
            },
            {
                text: "儲位位置",
                dataIndex: 'F4',
                width: 120,
                //renderer: function (value) {
                //    switch (value) {
                //        case '35': return '申購單開立'; break;
                //        case '34': return '申購進貨完成'; break;
                //        case '36': return '已轉訂單'; break;
                //        default: return value; break;
                //    }
                //}
            },
            //{
            //    text: "現存數量",
            //    dataIndex: 'F5',
            //    width: 120
            //},
            //{
            //    //xtype: 'datecolumn',
            //    text: "末效期",
            //    dataIndex: 'F8',
            //    width: 100,
            //    //format: 'Xmd'
            //},
            {
                text: "買斷寄庫",
                dataIndex: 'F6',
                width: 90
            },
            //{
            //    text: "批號",
            //    dataIndex: 'F9',
            //    width: 180
            //},
            {
                text: "備註",
                dataIndex: 'F7',
                width: 180
            },
            {
                header: "",
                flex: 1
            }
        ],
        //viewConfig: {
        //    listeners: {
        //        refresh: function (view) {
        //        }
        //    }
        //},
        listeners: {
            selectionchange: function (model, records) {
                msglabel('訊息區:');
                //if (Ext.getCmp('eastform').collapsed == true) {
                //    Ext.getCmp('eastform').expand();
                //}
                T1Rec = records.length;
                T1LastRec = records[0];
                setFormT1a();
            }
        }
    });

    // 按'新增'或'修改'時的動作
    function setFormT1(x, t) {
        viewport.down('#t1Grid').mask();
        viewport.down('#form').setTitle(t);
        viewport.down('#form').expand();
        var f = T1Form.getForm();
        if (x === "I") {
            var r = Ext.create('T1Model');
            T1Form.loadRecord(r); // 建立空白model,在新增時載入T1Form以清空欄位內容
            //f.findField("F2").setValue('(系統自編)');
            //f.findField("F3").setReadOnly(false);
            //f.findField("F4").clearInvalid();
            //f.findField("F5").setValue('01');
            //f.findField("F6").setValue('35');
            //f.findField("PR_TIME").setValue(currentDate);
            //f.findField("CREATE_USER").setValue(userName);
            f.findField("WH_NO").setReadOnly(false);
            f.findField("STORE_LOC").setReadOnly(false);
            setCmpShowCondition(true, true, true, true);
        } else {
            setCmpShowCondition(false, true, true, true);
            f.findField("F3").setReadOnly(false);
            f.findField("STORE_LOC").setReadOnly(false);
            //f.findField("WH_NO").setReadOnly(false);

        }

        f.findField('x').setValue(x);
        T1Form.down('#cancel').setVisible(true);
        T1Form.down('#submit').setVisible(true);
    }

    function showComponent(form, fieldName) {
        var u = form.findField(fieldName);
        u.show();
    }

    function hideComponent(form, fieldName) {
        var u = form.findField(fieldName);
        u.hide();
    }

    //控制不可更改項目的顯示
    function setCmpShowCondition(F2, F4, F5, F7) {
        var f = T1Form.getForm();
        F2 ? showComponent(f, "MMCODE") : hideComponent(f, "MMCODE");
        !F2 ? showComponent(f, "MMCODE_TEXT") : hideComponent(f, "MMCODE_TEXT");
        F4 ? showComponent(f, "STORE_LOC") : hideComponent(f, "STORE_LOC");
        !F4 ? showComponent(f, "STORE_LOC_TEXT") : hideComponent(f, "STORE_LOC_TEXT");
       // F5 ? showComponent(f, "INV_QTY") : hideComponent(f, "INV_QTY");
       // !F5 ? showComponent(f, "INV_QTY_TEXT") : hideComponent(f, "INV_QTY_TEXT");
        F7 ? showComponent(f, "LOC_NOTE") : hideComponent(f, "LOC_NOTE");
        !F7 ? showComponent(f, "LOC_NOTE_TEXT") : hideComponent(f, "LOC_NOTE_TEXT");
    }

    // 顯示明細/新增/修改輸入欄
    var T1Form = Ext.widget({
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
        defaultType: 'textfield',
        items: [
            {
                fieldLabel: 'Update',
                name: 'x',
                xtype: 'hidden'
            },
            {
                name: 'ORI_STORE_LOC',
                xtype: 'hidden',
                submitValue: true
            }, {
                xtype: 'combo',
                fieldLabel: '庫房代碼',
                name: 'WH_NO',
                store: T1QueryWhnoCombo,
                queryMode: 'local',
                displayField: 'TEXT',
                valueField: 'VALUE',
                tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{VALUE} {TEXT}&nbsp;</div></tpl>',
                anyMatch: true,
                readOnly: true,
                allowBlank: false, // 欄位為必填
                typeAhead: true,
                forceSelection: true,
                queryMode: 'local',
                triggerAction: 'all',
                fieldCls: 'required'
            },

            //{
            //    fieldLabel: '庫房代碼',
            //    allowBlank: false,
            //    name: 'WH_NO',
            //    submitValue: true
            //},
            T1FormMMCode,
            //{
            //    fieldLabel: '藥材代碼',
            //    name: 'MMCODE',
            //    allowBlank: false,
            //    fieldCls: 'required',
            //    submitValue: true
            //},
            {
                xtype: 'displayfield',
                fieldLabel: '藥材代碼',
                name: 'MMCODE_TEXT'
                //readOnly: true
            },
            {
                xtype: 'displayfield',
                fieldLabel: '藥材名稱',
                name: 'F3',
            },
            //{
            //    fieldLabel: '儲存位置',
            //    name: 'STORE_LOC',
            //    fieldCls: 'required',
            //    submitValue: true
            //},
            {
                xtype: 'textfield',
                fieldLabel: '儲存位置',
                name: 'STORE_LOC',
                fieldCls: 'required',
                readOnly: true,
                submitValue: true
            },
            {
                xtype: 'displayfield',
                fieldLabel: '儲存位置',
                name: 'STORE_LOC_TEXT'
            },
            //{
            //    fieldLabel: '現存數量',
            //    name: 'INV_QTY',
            //    fieldCls: 'required'
            //},
            //{
            //    xtype: 'displayfield',
            //    fieldLabel: '現存數量',
            //    name: 'INV_QTY_TEXT'
            //},
            //{
            //    xtype: 'displayfield',
            //    fieldLabel: '末效期',
            //    name: 'F8'
            //},
            {
                xtype: 'displayfield',
                fieldLabel: '買斷寄庫',
                name: 'F6'
            },
            //{
            //    xtype: 'displayfield',
            //    fieldLabel: '批號',
            //    name: 'F9'
            //},
            {
                xtype: 'textarea',
                fieldLabel: '備註',
                name: 'LOC_NOTE',
            },
            {
                xtype: 'displayfield',
                fieldLabel: '備註',
                name: 'LOC_NOTE_TEXT'
            },
        ],
        buttons: [
            {
                itemId: 'submit', text: '儲存', hidden: true,
                handler: function () {
                    if (this.up('form').getForm().isValid()) {
                        var confirmSubmit = viewport.down('#form').title.substring(0, 2);
                        Ext.MessageBox.confirm(confirmSubmit, '是否確定' + confirmSubmit + '?', function (btn, text) {
                            if (btn === 'yes') {
                                T1Submit();
                            }
                        });
                    }
                    else
                        Ext.Msg.alert('提醒', '輸入資料格式有誤');
                }
            },
            {
                itemId: 'cancel', text: '取消', hidden: true, handler: T1Cleanup
            }
        ]
    });

    function T1Submit() {
        var f = T1Form.getForm();
        if (f.isValid()) {
            var myMask = new Ext.LoadMask(viewport, { msg: '處理中...' });
            myMask.show();
            f.submit({
                url: T1Set,
                success: function (form, action) {
                    myMask.hide();
                    var f2 = T1Form.getForm();
                    var r = f2.getRecord();
                    var msg = action.result.msg;

                    switch (f2.findField("x").getValue()) {
                        case "I":
                            var TQ = T1Query.getForm();

                            //TQ.findField('P0').setValue(f2.findField('MAT_CLASS').getValue());
                            //TQ.findField('P1').setValue(new Date());
                            //TQ.findField('P2').setValue(new Date());

                            T1Load();
                            //msglabel(msg);

                            Ext.Msg.alert('訊息', msg);
                            break;
                        case "U":
                            msglabel('訊息區:資料更新成功');
                            T1Load();
                            break;
                    }

                    T1Cleanup();
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

    function T1Cleanup() {
        viewport.down('#t1Grid').unmask();
        Ext.getCmp('eastform').collapse();
        var f = T1Form.getForm();
        f.reset();
        //f.getFields().each(function (fc) {
        //    if (fc.xtype === "displayfield" || fc.xtype === "textfield") {
        //        fc.setReadOnly(true);
        //    } else if (fc.xtype === "combo" || fc.xtype === "datefield") {
        //        fc.setReadOnly(true);
        //    }
        //});
        T1Form.down('#cancel').hide();
        T1Form.down('#submit').hide();
        viewport.down('#form').setTitle('瀏覽');
        setCmpShowCondition(false, false, false, false);
      //  checkboxT1Model.deselectAll();
        setFormT1a();
    }

    var viewport = Ext.create('Ext.Viewport', {
        renderTo: body,
        layout: {
            type: 'border',
            padding: 0
        },
        defaults: {
            split: true  //可以調整大小
        },
        items: [
            {
                itemId: 't1Grid',
                region: 'center',
                layout: 'fit',
                collapsible: false,
                title: '',
                border: false,
                items: [T1Grid]
            },
            {
                itemId: 'form',
                id: 'eastform',
                region: 'east',
                collapsible: true,
                floatable: true,
                width: 300,
                title: '瀏覽',
                border: false,
                collapsed: true,
                layout: {
                    type: 'fit',
                    padding: 5,
                    align: 'stretch'
                },
                items: [T1Form]
            }
        ]
    });

    setCmpShowCondition(false, false, false, false);

    T1Query.getForm().findField('P4').setValue('N');
});