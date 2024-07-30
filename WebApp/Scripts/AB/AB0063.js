Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.require(['WEBAPP.utils.Common']);

Ext.onReady(function () {
    Ext.override(Ext.grid.column.Column, { menuDisabled: true });
    var T1Get = '/api/AB0063/GetQueryData';
    var getMatClass = '../../../api/AB0063/GetMatClass';
    var getWh = '../../../api/AB0063/GetWH';
    var getExcel = '../../../api/AA0072/Excel';
    var reportUrl = '/Report/A/AA0072.aspx';
    var T1RecLength = 0;
    var T1LastRec = null;
    var vMatClass = "";
    var vPrintTitle = "";
    var vDataYMFrom = "";
    var vDataYMTo = "";
    var vWhNo = "";
    var vMmCode = "";
    var vDocType = "";

    var storeMatClass = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'COMBITEM']
    });

    var storeWhNo = Ext.create('Ext.data.Store', {
        fields: ['WH_NO', 'WH_NAME']
    });

    Ext.define('T1Model', {
        extend: 'Ext.data.Model',
        fields: [
            'MMCODE',
            'MMNAME_C',
            'MMNAME_E',
            'BASE_UNIT',
            'WH_NO',
            'DATA_YM',
            'APL_OUTQTY',
            'AVG_PRICE',
            'LUMP_SUM'
        ]
    });

    var T1QueryMMCode = Ext.create('WEBAPP.form.MMCodeCombo', {
        name: 'mmCode',
        fieldLabel: '院內碼',
        labelAlign: 'right',
        limit: 50, //限制一次最多顯示10筆
        queryUrl: '/api/AB0063/GetMMCodeCombo', //指定查詢的Controller路徑
        extraFields: ['MMNAME_C', 'MMNAME_E'], //查詢完會回傳的欄位
        getDefaultParams: function () { //查詢時Controller固定會收到的參數
        },
        listeners: {
            select: function (c, r, i, e) {
                //選取下拉項目時，顯示回傳值
            }
        }
    });

    var T1Store = Ext.create('Ext.data.Store', {
        model: 'T1Model',
        pageSize: 20,
        //autoLoad: true,
        remoteSort: true,
        sorters: [{ property: 'WH_NO', direction: 'ASC' }, { property: 'MAT_CLASS', direction: 'ASC' }, { property: 'MMCODE', direction: 'ASC' }, { property: 'DATA_YM', direction: 'ASC' }],
        listeners: {
            beforeload: function (store, options) {
                var np = {
                    matClass: vMatClass,
                    dataYMFrom: vDataYMFrom,
                    dataYMTo: vDataYMTo,
                    whNo: vWhNo,
                    mmCode: vMmCode,
                    docType: vDocType
                };
                Ext.apply(store.proxy.extraParams, np);
            }
        },

        proxy: {
            type: 'ajax',
            timeout: 1800000,
            actionMethods: {
                create: 'POST',
                read: 'POST',
                update: 'PUT',
                delete: 'DELETE'
            },
            url: T1Get,
            reader: {
                type: 'json',
                rootProperty: 'etts',
                totalProperty: 'rc'
            }
        }
    });

    //庫房代碼
    function setComboWH() {
        Ext.Ajax.request({
            url: getWh,
            method: reqVal_p,
            success: function (response) {
                var data = Ext.decode(response.responseText);

                if (data.success) {
                    var tb_wh_no = data.etts;
                    var combo_P0 = T1Query.getForm().findField('whNo');


                    if (tb_wh_no.length === 1) {
                        storeWhNo.add({ WH_NO: tb_wh_no[0].WH_NO, WH_NAME: tb_wh_no[0].WH_NAME });
                        combo_P0.setValue(tb_wh_no[0].WH_NO);
                    }
                    else {
                        combo_P0.setDisabled(false);
                        storeWhNo.add({ WH_NO: '全部', WH_NAME: '' });
                        if (tb_wh_no.length > 0) {
                            for (var i = 0; i < tb_wh_no.length; i++) {
                                storeWhNo.add({ WH_NO: tb_wh_no[i].WH_NO, WH_NAME: tb_wh_no[i].WH_NAME });
                            }
                        }
                    }
                }
            }
        });
    }

    //物料分類
    function setComboMatClass() {
        Ext.Ajax.request({
            url: getMatClass,
            params: {
                whNo: vWhNo
            },

            method: reqVal_p,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    storeMatClass.removeAll();
                    T1Query.getForm().findField('matClass').setValue('');
                    var tb_matclass = data.etts;

                    if (tb_matclass.length > 0) {
                        for (var i = 0; i < tb_matclass.length; i++) {
                            storeMatClass.add({ VALUE: tb_matclass[i].VALUE, COMBITEM: tb_matclass[i].COMBITEM });
                        }

                        if (tb_matclass.length === 1) {
                            T1Query.getForm().findField('matClass').setValue(tb_matclass[0].VALUE);
                        }
                        else {
                            T1Query.getForm().findField('matClass').setDisabled(false);
                        }
                    }
                }
            }
        });
    }
    setComboMatClass();

    function T1Load() {
        T1Tool.moveFirst();
        msglabel('訊息區:');
    }

    function showReport() {
        if (!win) {
            var winform = Ext.create('Ext.form.Panel', {
                id: 'iframeReport',
                layout: 'fit',
                closable: false,
                html: '<iframe src="' + reportUrl
                + '?matClass=' + vMatClass
                + '&fromYM=' + vDataYMFrom
                + '&toYM=' + vDataYMTo
                + '&whNo=' + vWhNo
                + '&mmCode=' + vMmCode
                + '&docType=' + vDocType
                + '&printTitle=' + vPrintTitle
                + '&frompgm=AB0063' 
                + '" id="mainContent" name="mainContent" width="100%" height="100%" frameborder="0" style="background-color:#FFFFFF"></iframe>',
                buttons: [{
                    text: '關閉',
                    handler: function () {
                        this.up('window').destroy();
                    }
                }]
            });
            var win = GetPopWin(viewport, winform, '', viewport.width - 20, viewport.height - 20);
        }
        win.show();
    }

    setComboWH();

    var mLabelWidth = 60;
    var mWidth = 180;
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
                    xtype: 'combo',
                    store: storeWhNo,
                    fieldLabel: '庫房代碼',
                    name: 'whNo',
                    id: 'whNo',
                    width: 200,
                    padding: '0 4 0 4',
                    queryMode: 'local',
                    anyMatch: true,
                    autoSelect: true,
                    displayField: 'WH_NO',
                    valueField: 'WH_NO',
                    tpl: new Ext.XTemplate(
                        '<tpl for=".">',
                        '<tpl if="VALUE==\'\'">',
                        '<div class="x-boundlist-item" style="height:auto;">{WH_NO}&nbsp;</div>',
                        '<tpl else>',
                        '<div class="x-boundlist-item" style="height:auto;border-bottom: 2px dashed #0a0;">' +
                        '<span style="color:red">{WH_NO}</span><br/>&nbsp;<span style="color:blue">{WH_NAME}</span></div>',
                        '</tpl></tpl>', {
                            formatText: function (text) {
                                return Ext.util.Format.htmlEncode(text);
                            }
                        }),
                    listeners: {
                        beforequery: function (record) {
                            record.query = new RegExp(record.query, 'i');
                            record.forceAll = true;
                        },
                        change: function (model, records) {
                            vWhNo = T1Query.getForm().findField('whNo').getValue();
                            // setComboMatClass();
                        }
                    }
                }, T1QueryMMCode, {
                    xtype: 'monthfield',
                    fieldLabel: '核撥年月',
                    name: 'dataYMFrom',
                    id: 'dataYMFrom',
                    width: 130,
                    labelWidth: 60,
                    padding: '0 10 0 20',
                    fieldCls: 'required'
                }, {
                    xtype: 'monthfield',
                    fieldLabel: '至',
                    name: 'dataYMTo',
                    id: 'dataYMTo',
                    width: 80,
                    labelWidth: 10,
                    padding: '0 20 0 0',
                    fieldCls: 'required'
                }, {
                    xtype: 'combo',
                    fieldLabel: '物料分類',
                    name: 'matClass',
                    id: 'matClass',
                    store: storeMatClass,
                    queryMode: 'local',
                    displayField: 'COMBITEM',
                    valueField: 'VALUE',
                    multiSelect: true,
                    queryMode: 'local',
                    anyMatch: true,
                    autoSelect: true,
                    listeners: {
                        beforequery: function (record) {
                            record.query = new RegExp(record.query, 'i');
                            record.forceAll = true;
                        }
                    }
                }, {
                    xtype: 'radiogroup',
                    anchor: '40%',
                    labelWidth: 25,
                    width: 180,
                    name: 'docType',
                    items: [
                        { boxLabel: '庫備', width: 50, name: 'docTypeRB', inputValue: 1, checked: true },
                        { boxLabel: '非庫備', width: 60, name: 'docTypeRB', inputValue: 0 },
                        { boxLabel: '全部品項', width: 70, name: 'docTypeRB', inputValue: 2 }
                    ],
                    listeners:
                    {
                        beforequery: function (record) {
                            record.query = new RegExp(record.query, 'i');
                            record.forceAll = true;
                            Ext.Msg.alert('說明', T1Query.getForm().findField('matClass').getValue());
                        }
                    }
                }, {
                    fieldLabel: 'Update',
                    name: 'matClassName',
                    xtype: 'hidden'
                }, {
                    xtype: 'button',
                    text: '查詢',
                    margin: '0 5 0 20',
                    handler: function () {
                        var msgStr = "";
                        var nowYM = (new Date().getFullYear() - 1911).toString();
                        var nowMM = new Date().getMonth() + 1;//getMonth 的1月為0

                        nowYM = nowYM + (nowMM > 9 ? '' : '0') + nowMM;
                        vMatClass = T1Query.getForm().findField('matClass').getValue();
                        vDataYMFrom = T1Query.getForm().findField('dataYMFrom').getValue();
                        vDataYMTo = T1Query.getForm().findField('dataYMTo').getValue();
                        vWhNo = T1Query.getForm().findField('whNo').getValue();
                        vMmCode = T1Query.getForm().findField('mmCode').getValue();
                        vDocType = T1Query.getForm().findField('docType').getChecked()[0].inputValue;
                        

                        if (vDataYMFrom === null) {
                            msgStr += (msgStr.length === 0 ? "請點選" : "、") + "核撥年月之起始";
                        }
                        else {
                            vDataYMFrom = (vDataYMFrom.getFullYear() - 1911).toString() + ((vDataYMFrom.getMonth() + 1) > 9 ? '' : '0') + (vDataYMFrom.getMonth() + 1);
                        }

                        if (vDataYMTo === null) {
                            if (vDataYMFrom !== null) {
                                vDataYMTo = nowYM;
                            }
                            else {
                                msgStr += (msgStr.length === 0 ? "請點選" : "、") + "核撥年月之起訖";
                            }
                        }
                        else {
                            vDataYMTo = (vDataYMTo.getFullYear() - 1911).toString() + (vDataYMTo.getMonth() + 1 > 9 ? '' : '0') + (vDataYMTo.getMonth() + 1);
                        }

                        if (vDocType === null) {
                            msgStr += (msgStr.length === 0 ? "請點選" : "、") + "庫備/非庫備";
                        }

                        if (msgStr.length > 0) {
                            Ext.Msg.alert('訊息', msgStr);
                        }
                        else {
                            if (vDataYMFrom > nowYM && vDataYMTo > nowYM) {
                                msgStr = "今日年月為" + nowYM + ", 請點選有效之起訖核撥年月";
                            }
                            else if (vDataYMFrom > vDataYMTo) {
                                msgStr = "核撥年月之起始年月須小於訖止年月";
                            }

                            if (msgStr.length > 0) {
                                Ext.Msg.alert('訊息', msgStr);
                            }
                            else {
                                vPrintTitle = vDataYMFrom + "~" + vDataYMTo + "(" + (vDocType === 2 ? "全部品項" : (vDocType === 1 ? "庫備品" : "非庫備品")) + ")";
                                T1Load();
                            }
                        }
                    }
                }, {
                    xtype: 'button',
                    text: '清除',
                    handler: function () {
                        var f = this.up('form').getForm();
                        var classStore = f.findField('matClass').getStore();
                        var whStore = f.findField('whNo').getStore();
                        var mmStore = f.findField('mmCode').getStore();
                        var firstClass = classStore.getAt(0);
                        var firstWh = whStore.getAt(0);

                        vMatClass = "";
                        vPrintTitle = "";
                        vDataYMFrom = "";
                        vDataYMTo = "";
                        vWhNo = "";
                        vMmCode = "";
                        vDocType = "";
                        f.reset();

                        //if (classStore.getCount() === 1) {
                        //    f.findField('matClass').setValue(firstClass.get('VALUE'))
                        //}
                        
                        //if (whStore.getCount() === 1) {
                        //    f.findField('whNo').setValue(firstWh.get('WH_NO'))
                        //}

                        f.findField('matClass').focus(); // 進入畫面時輸入游標預設在D0
                        msglabel('訊息區:');
                    }
                }
            ]
        }]
    });

    var T1Tool = Ext.create('Ext.PagingToolbar', {
        store: T1Store, //資料load進來
        displayInfo: true,
        border: false,
        plain: true,
        buttons: [
            {
                itemId: 'export', text: '匯出', //T1Query
                handler: function () {
                    var param = new Array();

                    if (T1Store.getCount() > 0) {
                        param.push({ name: 'FN', value: '單位申領明細報表' });
                        param.push({ name: 'matClass', value: vMatClass });
                        param.push({ name: 'dataYMFrom', value: vDataYMFrom });
                        param.push({ name: 'dataYMTo', value: vDataYMTo });
                        param.push({ name: 'whNo', value: vWhNo });
                        param.push({ name: 'mmCode', value: vMmCode });
                        param.push({ name: 'docType', value: vDocType });
                        param.push({ name: 'printTitle', value: vPrintTitle });
                        param.push({ name: 'frompgm', value: 'AB0063' });
                        PostForm(getExcel, param);
                        msglabel('訊息區:匯出完成');
                    }
                    else {
                        Ext.Msg.alert('訊息', '無資料可匯出');
                    }
                }
            }, {
                text: '列印', handler: function () {
                    if (T1Store.getCount() > 0) {
                        showReport();
                    }
                    else {
                        Ext.Msg.alert('訊息', '無資料可列');
                    }
                }
            }
        ]
    });

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
        columns: [
            {
                xtype: 'rownumberer'
            }, {
                text: "庫房代碼",
                dataIndex: 'WH_NO',
                style: 'text-align:left',
                align: 'left',
                width: 80
            }, {
                text: "庫房名稱",
                dataIndex: 'WH_NAME',
                style: 'text-align:left',
                align: 'left',
                width: 100
            }, {
                text: "院內碼",
                dataIndex: 'MMCODE',
                style: 'text-align:left',
                align: 'left',
                width: 100
            }, {
                text: "中文品名",
                dataIndex: 'MMNAME_C',
                style: 'text-align:left',
                align: 'left',
                width: 250
            }, {
                text: "英文品名",
                dataIndex: 'MMNAME_E',
                style: 'text-align:left',
                align: 'left',
                width: 250
            }, {
                text: "計量單位",
                dataIndex: 'BASE_UNIT',
                style: 'text-align:left',
                align: 'left',
                width: 80
            }, {
                text: "核撥年月",
                dataIndex: 'DATA_YM',
                style: 'text-align:left',
                align: 'left',
                width: 100
            }, {
                text: "核撥總量",
                dataIndex: 'APL_OUTQTY',
                style: 'text-align:left',
                align: 'right',
                width: 100
            }, {
                text: "撥發單價",
                dataIndex: 'AVG_PRICE',
                style: 'text-align:left',
                align: 'right',
                width: 100
            }, {
                text: "撥發成本",
                dataIndex: 'LUMP_SUM',
                style: 'text-align:left',
                align: 'right',
                width: 100
            }],
        listeners: {
            selectionchange: function (model, records) {
                T1RecLength = records.length;
                T1LastRec = records[0];
            }
        }
    });

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

    T1Query.getForm().findField('dataYMFrom').focus();
    var ymNow = (new Date().getFullYear() - 1911).toString() + ((new Date().getMonth() + 1) > 9 ? '' : '0') + (new Date().getMonth() + 1);
    T1Query.getForm().findField('dataYMFrom').setValue(ymNow);
    T1Query.getForm().findField('dataYMTo').setValue(ymNow);
});